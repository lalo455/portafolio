using System.Collections.Generic;
using System.Threading.Tasks;
using Encuesta.Core.Entities;

namespace Encuesta.Core.Interfaces
{
    public interface ISurveyService
    {
        Task<IEnumerable<Survey>> GetAllActiveAsync();
        Task<Survey?> GetByIdAsync(int id);
        Task<int> CreateAsync(Survey survey);
        Task DeactivateAsync(int id);
    }
}
