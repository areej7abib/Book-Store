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
    public class ApplicationRepository : Repository<ApplicationUser>, IApplicationRepository
    {
        private readonly ApplicationContext context;

        public ApplicationRepository(ApplicationContext context) : base(context)
        {
            this.context = context;
        }
    }
}
