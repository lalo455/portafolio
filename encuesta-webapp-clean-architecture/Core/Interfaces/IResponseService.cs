using System.Collections.Generic;
using System.Threading.Tasks;
using Encuesta.Core.Entities;

namespace Encuesta.Core.Interfaces
{
    public interface IResponseService
    {
        Task<int> SubmitResponseAsync(SurveyResponse response, IEnumerable<Answer> answers);
        Task<IEnumerable<SurveyResponse>> GetResponsesBySurveyIdAsync(int surveyId);
        Task<IEnumerable<Answer>> GetAnswersBySurveyIdAsync(int surveyId);
    }
}
