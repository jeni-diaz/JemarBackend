using Jemar.Application.Abstractions.Infrastructure;
using Jemar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jemar.Infrastructure.Persistence.Repository
{
    public class BaseRepository<T> : IBaseRepository<T>
        where T : BaseEntity
    {
        protected readonly JemarDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(JemarDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual List<T> GetAll()
        {
            return _dbSet
                .Where(x => !x.IsDeleted)
                .ToList();
        }

        public virtual T? GetById(Guid id)
        {
            return _dbSet
                .FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        }

        public virtual T Add(T entity)
        {
            entity.UpdatedDateTime = DateTime.UtcNow;

            _dbSet.Add(entity);

            SaveChanges();

            return entity;
        }

        public virtual void Update(T entity)
        {
            entity.UpdatedDateTime = DateTime.UtcNow;

            _dbSet.Update(entity);

            SaveChanges();
        }

        public virtual void Delete(Guid id)
        {
            var entity = GetById(id);

            if (entity != null)
            {
                entity.IsDeleted = true;

                entity.DeletedDateTime = DateTime.UtcNow;

                entity.UpdatedDateTime = DateTime.UtcNow;

                _dbSet.Update(entity);

                SaveChanges();
            }
        }

        protected void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}