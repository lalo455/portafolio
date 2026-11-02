// Interfaces/ISapIntegrationService.cs
using Hangfire;

namespace SapMiddleware.Interfaces;

public interface ISapIntegrationService
{
    /// <summary>
    /// Procesa la creación de un Pedido (oOrders) en SAP B1.
    /// Reintentos automáticos: hasta 3 veces con backoff progresivo (30s → 60s → 120s).
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task ProcessPedidoAsync(Guid ticketId);

    /// <summary>
    /// Procesa la creación de una Orden de Entrega (oDeliveryNotes) en SAP B1 basada en un pedido.
    /// Reintentos automáticos: hasta 3 veces con backoff progresivo (30s → 60s → 120s).
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task ProcessEntregaAsync(Guid ticketId);
}
