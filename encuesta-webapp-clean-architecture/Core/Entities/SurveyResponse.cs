using System;
using System.Collections.Generic;

namespace Encuesta.Core.Entities
{
    public class SurveyResponse
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        
        // Relación
        public Survey Survey { get; set; } = null!;
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
