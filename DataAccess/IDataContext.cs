using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EliotJones.AspIdentity.DataAccess
{
    public interface IDataContext
    {
        Task CreateAsync<T>(T item) where T : class;

        Task DeleteAsync<T>(T item) where T : class;

        Task<T> GetByIdAsync<T>(Guid id) where T : class;

        Task<IReadOnlyCollection<T>> GetAllAsync<T>() where T : class;

        Task UpdateAsync<T>(T item) where T : class;
    }
}
