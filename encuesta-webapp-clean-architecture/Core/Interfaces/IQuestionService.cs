using System.Collections.Generic;
using System.Threading.Tasks;
using Encuesta.Core.Entities;

namespace Encuesta.Core.Interfaces
{
    public interface IQuestionService
    {
        Task<IEnumerable<Question>> GetBySurveyIdAsync(int surveyId);
        Task<Question?> GetByIdAsync(int id);
        Task<int> CreateAsync(Question question);
        Task UpdateAsync(Question question);
        Task DeleteAsync(int id);
        Task<IEnumerable<QuestionOption>> GetOptionsByQuestionIdAsync(int questionId);
    }
}
