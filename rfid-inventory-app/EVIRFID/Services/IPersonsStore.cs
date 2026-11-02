using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EVIRFID.Services
{
    public interface IPersonsStore<T>
    {
        //Items
        Task<bool> AddPersonAsync(T person);
        Task<bool> UpdatePersonAsync(T person);
        Task<bool> DeletePersonAsync(int id);
        Task<T> GetPersonAsync(int id);
        Task<IEnumerable<T>> GetPersonsAsync(bool forceRefresh = false);
    }
}