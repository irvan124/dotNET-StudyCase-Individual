using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrollment_Service.Data
{
    public interface ICrud<T>
    {
        Task<IEnumerable<T>> GetAll();

        Task<T> GetById(string id);
        Task<T> Insert(T obj);
        Task<T> Update(string id, T obj);
        Task Delete(string id);
    }
}