using E_commerce.DataAccess.Repository.IRepository;
using E_Commerce.Models;
using Ecommerce.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.DataAccess.Repository
{
    public class CategryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationContext dB;

        public CategryRepository(ApplicationContext _DB):base(_DB)
        {
            dB = _DB;
        }

        public void Update(Category category)
        {
           dB.Categories.Update(category);
        }

      

    }
}
