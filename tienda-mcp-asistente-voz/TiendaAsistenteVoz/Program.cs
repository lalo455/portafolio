// ╔══════════════════════════════════════════════════════════════════════╗
// ║  Program.cs — Frontend: La Tiendita del Barrio                    ║
// ╠══════════════════════════════════════════════════════════════════════╣
// ║                                                                    ║
// ║  📚 ¿QUÉ HACE ESTE PROYECTO AHORA?                                ║
// ║                                                                    ║
// ║  Se ha simplificado al máximo. Ahora solo es un servidor de       ║
// ║  archivos estáticos (HTML, CSS, JS). Toda la lógica pesada        ║
// ║  (Gemini, SQLite, MCP) se movió al WebApi y al MCP Server.        ║
// ║                                                                    ║
// ║  🚀 PUERTO: 5030                                                   ║
// ╚══════════════════════════════════════════════════════════════════════╝

var builder = WebApplication.CreateBuilder(args);

// Configuramos el puerto 5030 para el frontend
builder.WebHost.UseUrls("http://localhost:5030");

var app = builder.Build();

// Habilitar servir archivos desde wwwroot (index.html, app.js, styles.css)
app.UseStaticFiles();

// Cualquier ruta no encontrada redirige al index.html
app.MapFallbackToFile("index.html");

Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║  🖥️  Frontend — La Tiendita del Barrio          ║");
Console.WriteLine("║  📡 Puerto: 5030                                 ║");
Console.WriteLine("║  🌐 URL: http://localhost:5030                   ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");

app.Run();