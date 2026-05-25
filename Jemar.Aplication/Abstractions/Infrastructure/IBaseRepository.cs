using System;
using System.Collections.Generic;
using System.Text;

namespace Jemar.Application.Abstractions.Infrastructure
{
    public interface IBaseRepository<T> where T : class
    {
        List<T> GetAll();

        T? GetById(Guid id);

        T Add(T entity);

        void Update(T entity);

        void Delete(Guid id);
    }
}