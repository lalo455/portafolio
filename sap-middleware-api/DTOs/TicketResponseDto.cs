namespace SapMiddleware.DTOs
{
    public class TicketResponseDto
    {
      
            public Guid TicketId { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        
    }
}
