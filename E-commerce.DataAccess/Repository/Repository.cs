using System.Linq.Expressions;
using E_commerce.DataAccess.Repository.IRepository;
using Ecommerce.DataAccess.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace E_commerce.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationContext context;
        internal DbSet<T> _dbset;

        public Repository(ApplicationContext context)
        {
            this.context = context;
            this._dbset = context.Set<T>();
        }

        public void Add(T entity)
        {
            _dbset.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter, string? IncludeProperities = null, bool tracked = false)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = _dbset;

            }
            else
            {
                query = _dbset.AsNoTracking();
            }
            query = query.Where(filter);
            if (!string.IsNullOrEmpty(IncludeProperities))
            {
                foreach (var item in IncludeProperities
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            T? Entity = query.FirstOrDefault();  
            return Entity;

        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? IncludeProperities = null)
        {
            IQueryable<T> query = _dbset;
            if (filter != null)
                query = query.Where(filter);
            if (!string.IsNullOrEmpty(IncludeProperities))
            {
                foreach (var item in IncludeProperities
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            return query.ToList();
        }

        public T GetById(int? id)
        {
            T? entity = _dbset.Find(id);
            if (entity == null)
            {
                throw new NullReferenceException();
            }
            return entity;
        }

        public void Delete(T entity)
        {
            _dbset.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbset.RemoveRange(entities);
        }

    }
}
