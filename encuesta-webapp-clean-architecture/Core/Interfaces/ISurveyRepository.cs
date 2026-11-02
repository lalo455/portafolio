using System.Collections.Generic;
using System.Threading.Tasks;
using Encuesta.Core.Entities;

namespace Encuesta.Core.Interfaces
{
    public interface ISurveyRepository
    {
        Task<IEnumerable<Survey>> GetAllAsync();
        Task<Survey?> GetByIdAsync(int id);
        Task<int> AddAsync(Survey survey);
        Task UpdateAsync(Survey survey);
        Task DeactivateAsync(int id);
    }
}
