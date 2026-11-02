using System.Collections.Generic;
using System.Threading.Tasks;
using Encuesta.Core.Entities;

namespace Encuesta.Core.Interfaces
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<Question>> GetBySurveyIdAsync(int surveyId);
        Task<Question?> GetByIdAsync(int id);
        Task<int> AddAsync(Question question);
        Task UpdateAsync(Question question);
        Task DeleteAsync(int id);
        Task<int> AddOptionAsync(QuestionOption option);
        Task<IEnumerable<QuestionOption>> GetOptionsByQuestionIdAsync(int questionId);
        Task DeleteOptionsByQuestionIdAsync(int questionId);
    }
}
