using System.Collections.Generic;
using System.Threading.Tasks;
using Encuesta.Core.Entities;

namespace Encuesta.Core.Interfaces
{
    public interface IResponseRepository
    {
        Task<int> AddResponseAsync(SurveyResponse response);
        Task AddAnswerAsync(Answer answer);
        Task<IEnumerable<SurveyResponse>> GetResponsesBySurveyIdAsync(int surveyId);
        Task<IEnumerable<Answer>> GetAnswersBySurveyIdAsync(int surveyId);
    }
}
