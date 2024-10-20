using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.DataAccess.Repository.IRepository
{
    public  interface IRepository<T> where T : class
    {
        void Add(T entity);

        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null,string? IncludeProperities = null);
        T Get(Expression<Func<T, bool>> filter, string? IncludeProperities = null, bool tracked = false);
        T GetById(int? id);


        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);

    }
}
