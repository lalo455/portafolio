using System.Collections.Generic;

namespace Encuesta.Core.Entities
{
    public class Question
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public int Order { get; set; }
        
        // Relación
        public Survey Survey { get; set; } = null!;
        public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}
