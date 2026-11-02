namespace Encuesta.Core.Entities
{
    public class QuestionOption
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Value { get; set; } = string.Empty;
        public string? Label { get; set; }
        public int Order { get; set; }
        
        // Relación
        public Question Question { get; set; } = null!;
    }
}
