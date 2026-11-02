namespace SapMiddleware.Models;

public class IntegrationTicket
{
    public Guid Id { get; set; }
    public string Payload { get; set; } = string.Empty;
    public string Status { get; set; } = "En Cola"; // En Cola, Procesando, Completado, Error
    public int? SapDocNum { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
