using E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.DataAccess.Repository.IRepository
{
    public interface IProductRepository:IRepository<Product>
    {
        void update(Product product);
    }
}
