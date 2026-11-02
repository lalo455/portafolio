// Program.cs
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog: logs por Sociedad/Usuario en archivos diarios ───────────────────
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Map("Sociedad", "Sistema", (sociedad, wt1) =>
            wt1.Map("Usuario", "Global", (usuario, wt2) =>
                wt2.File($"Logs/{sociedad}/{usuario}-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
        ));

string sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ── Hangfire: cola de jobs anti-caídas ───────────────────────────────────────
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(sqlConnectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout      = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout  = TimeSpan.FromMinutes(5),
        QueuePollInterval           = TimeSpan.Zero,   // Reactividad máxima: sin polling delay
        UseRecommendedIsolationLevel = true
    }));

builder.Services.AddHangfireServer(options =>
{
    // CRÍTICO: 1 worker = procesamiento secuencial seguro para COM/DI-API.
    // La DI-API de SAP B1 NO es thread-safe. Con 1 worker nunca habrá colisiones.
    // Si en el futuro se necesita más throughput, usar un SemaphoreSlim global antes de subir este valor.
    options.WorkerCount = 1;

    // Timeout de servidor: si el job tarda más de 10 minutos, Hangfire lo considera
    // muerto y lo re-encola (aplica los reintentos configurados en la interfaz).
    options.ServerTimeout = TimeSpan.FromMinutes(10);
});

// ── Dapper: conexión SQL por demanda (Transient = nueva conexión por operación) ─
builder.Services.AddTransient<IDbConnection>(sp => new SqlConnection(sqlConnectionString));

// ── Servicios de integración SAP ─────────────────────────────────────────────
// SapConnectionManager como Transient: cada job de Hangfire obtiene su propio
// manager, lo que garantiza que no haya estado compartido entre jobs concurrentes.
builder.Services.AddTransient<SapMiddleware.Interfaces.ISapConnectionManager,
                              SapMiddleware.Services.SapConnectionManager>();

// SapIntegrationService como Scoped: Hangfire crea un scope por job,
// por lo que cada ejecución obtiene una instancia limpia e independiente.
builder.Services.AddScoped<SapMiddleware.Interfaces.ISapIntegrationService,
                           SapMiddleware.Services.SapIntegrationService>();

// MigrationService: para ejecutar las migraciones al arrancar
builder.Services.AddTransient<SapMiddleware.Services.MigrationService>();

// ── CORS: permitir que los sistemas web (Rastreo, CRM) puedan llamar al API ──
// Se permite cualquier origen porque los clientes son sistemas internos de red local.
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title   = "SAP Middleware API",
        Version = "v1",
        Description = "API de integración SAP B1 — Pedidos (CRM) y Entregas (Rastreo)"
    });
});

var app = builder.Build();

// ── Aplicar migraciones automáticas al arrancar ──────────────────────────────
using (var scope = app.Services.CreateScope())
{
    try
    {
        var migrationService = scope.ServiceProvider.GetRequiredService<SapMiddleware.Services.MigrationService>();
        await migrationService.ApplyMigrationsAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al aplicar migraciones (¿SQL Server inaccesible?): {ex.Message}");
    }
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
// Se quita la restricción IsDevelopment() para que Swagger cargue siempre (incluso en IIS local).
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    // CRÍTICO: Usar ruta relativa "v1/swagger.json" en lugar de "/swagger/v1/swagger.json".
    // Esto evita que IIS busque el archivo en "localhost/swagger..." en lugar de "localhost/apiSap/swagger..."
    c.SwaggerEndpoint("v1/swagger.json", "SAP Middleware v1");
});

// NOTA: UseHttpsRedirection eliminado intencionalmente.
// El API corre sobre HTTP en IIS sin certificado SSL.
// Activarlo causaba "Failed to fetch" al redirigir HTTP → HTTPS sin destino válido.

app.UseCors("PermitirTodo");
app.UseAuthorization();

// Dashboard Hangfire (acceso permitido en red local/desarrollo)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
});

public class AllowAllDashboardAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        return true;
    }
}

app.MapControllers();

app.Run();