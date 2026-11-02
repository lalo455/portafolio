namespace Encuesta.Core.Entities
{
    public class Answer
    {
        public int Id { get; set; }
        public int SurveyResponseId { get; set; }
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }
        public string? TextValue { get; set; }
        
        // Relaciones
        public SurveyResponse SurveyResponse { get; set; } = null!;
        public Question Question { get; set; } = null!;
        public QuestionOption? SelectedOption { get; set; }
    }
}
