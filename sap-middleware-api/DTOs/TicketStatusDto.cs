namespace SapMiddleware.DTOs
{
    public class TicketStatusDto
    {
        public Guid TicketId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? SapDocNum { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
