// Controllers/IntegrationController.cs
using Dapper;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using SapMiddleware.DTOs;
using SapMiddleware.Interfaces;
using System.Data;
using System.Text.Json;

namespace SapMiddleware.Controllers;

[ApiController]
[Route("api/integracion")]
public class IntegrationController : ControllerBase
{
    private readonly IDbConnection _dbConnection;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<IntegrationController> _logger;

    public IntegrationController(
        IDbConnection dbConnection,
        IBackgroundJobClient backgroundJobClient,
        ILogger<IntegrationController> logger)
    {
        _dbConnection     = dbConnection;
        _backgroundJobClient = backgroundJobClient;
        _logger           = logger;
    }

    // ─── POST /api/integracion/pedidos ─────────────────────────────────────
    [HttpPost("pedidos")]
    public async Task<IActionResult> IntegrarPedido([FromBody] PedidoIntegracionDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Clave de deduplicación para pedidos: número de pedido web único
        string claveDedup = request.U_NumPed;
        return await EncolarDocumento(request, "Pedido", claveDedup);
    }

    // ─── POST /api/integracion/entregas ────────────────────────────────────
    [HttpPost("entregas")]
    public async Task<IActionResult> IntegrarEntrega([FromBody] OrdenEntregaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validación adicional: debe haber al menos una línea marcada como entregada
        bool hayLineasEntregadas = request.Detalles.Any(d => d.Entregado && d.CantidadEditada > 0);
        if (!hayLineasEntregadas)
        {
            return BadRequest(new
            {
                Error = "No se enviaron líneas con Entregado = true y CantidadEditada > 0. No se creará entrega en SAP."
            });
        }

        // Clave de deduplicación para entregas: DocEntry del pedido origen
        string claveDedup = request.DocEntryPedido.ToString();
        return await EncolarDocumento(request, "Entrega", claveDedup);
    }

    // ─── GET /api/integracion/estatus/{ticketId} ───────────────────────────
    [HttpGet("estatus/{ticketId:guid}")]
    public async Task<IActionResult> GetTicketStatus(Guid ticketId)
    {
        const string sqlQuery = @"
            SELECT  Id            AS TicketId,
                    TipoDocumento AS Tipo,
                    Status,
                    SapDocNum,
                    SapDocEntry,
                    ErrorMessage,
                    CreatedAt,
                    LastUpdatedAt
            FROM    IntegrationTickets
            WHERE   Id = @Id";

        var status = await _dbConnection.QuerySingleOrDefaultAsync(sqlQuery, new { Id = ticketId });

        if (status == null) return NotFound(new { Error = $"Ticket {ticketId} no encontrado." });
        return Ok(status);
    }

    // ─── GET /api/integracion/health ───────────────────────────────────────
    /// <summary>
    /// Endpoint de monitoreo: devuelve el resumen de tickets de las últimas 24 horas.
    /// Útil para verificar que el worker de Hangfire está procesando correctamente.
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> GetHealth()
    {
        const string sqlHealth = @"
            SELECT  Status,
                    TipoDocumento AS Tipo,
                    COUNT(*)      AS Total
            FROM    IntegrationTickets
            WHERE   CreatedAt >= DATEADD(HOUR, -24, GETUTCDATE())
            GROUP BY Status, TipoDocumento
            ORDER BY Status, TipoDocumento";

        var resumen = await _dbConnection.QueryAsync(sqlHealth);

        return Ok(new
        {
            Timestamp = DateTime.UtcNow,
            Resumen   = resumen
        });
    }

    // ─── Método privado: Encolar con deduplicación ─────────────────────────
    private async Task<IActionResult> EncolarDocumento(object payload, string tipoDocumento, string claveDeduplicacion)
    {
        // Deduplicación: rechazar si ya existe un ticket reciente activo para la misma clave
        const string sqlDuplicado = @"
            SELECT TOP 1 Id
            FROM   IntegrationTickets
            WHERE  TipoDocumento       = @Tipo
              AND  ClaveDeduplicacion  = @Clave
              AND  Status              IN ('En Cola', 'Procesando', 'Completado')
              AND  CreatedAt           > DATEADD(HOUR, -1, GETUTCDATE())";

        var ticketExistente = await _dbConnection.QuerySingleOrDefaultAsync<Guid?>(
            sqlDuplicado,
            new { Tipo = tipoDocumento, Clave = claveDeduplicacion });

        if (ticketExistente.HasValue)
        {
            _logger.LogWarning(
                "Solicitud duplicada detectada. Tipo: {Tipo}, Clave: {Clave}, TicketExistente: {TicketId}",
                tipoDocumento, claveDeduplicacion, ticketExistente.Value);

            return Conflict(new
            {
                TicketId  = ticketExistente.Value,
                Status    = "Duplicado",
                Tipo      = tipoDocumento,
                Mensaje   = "Ya existe un ticket reciente para esta operación. Use el TicketId para consultar el estatus."
            });
        }

        // Crear nuevo ticket
        var ticketId    = Guid.NewGuid();
        var jsonPayload = JsonSerializer.Serialize(payload);
        var now         = DateTime.UtcNow;

        const string sqlInsert = @"
            INSERT INTO IntegrationTickets
                (Id, TipoDocumento, ClaveDeduplicacion, Payload, Status, CreatedAt, LastUpdatedAt)
            VALUES
                (@Id, @TipoDocumento, @ClaveDeduplicacion, @Payload, @Status, @CreatedAt, @LastUpdatedAt)";

        await _dbConnection.ExecuteAsync(sqlInsert, new
        {
            Id                  = ticketId,
            TipoDocumento       = tipoDocumento,
            ClaveDeduplicacion  = claveDeduplicacion,
            Payload             = jsonPayload,
            Status              = "En Cola",
            CreatedAt           = now,
            LastUpdatedAt       = now
        });

        // Encolar en Hangfire
        if (tipoDocumento == "Pedido")
            _backgroundJobClient.Enqueue<ISapIntegrationService>(x => x.ProcessPedidoAsync(ticketId));
        else
            _backgroundJobClient.Enqueue<ISapIntegrationService>(x => x.ProcessEntregaAsync(ticketId));

        _logger.LogInformation(
            "Ticket encolado. Id: {TicketId}, Tipo: {Tipo}, Clave: {Clave}",
            ticketId, tipoDocumento, claveDeduplicacion);

        return Accepted(new
        {
            TicketId = ticketId,
            Status   = "En Cola",
            Tipo     = tipoDocumento
        });
    }
}