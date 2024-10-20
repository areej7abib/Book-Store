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
    public class CompanyRepository : Repository<Company> , ICompanyRepository
    {
        private readonly ApplicationContext applicationContext;

        public CompanyRepository(ApplicationContext applicationContext) : base(applicationContext)
        {            
                this.applicationContext = applicationContext;
        }

        public void Update(Company company)
        {
            if (company != null)
            {
                applicationContext.companies.Update(company);
            }
        }
    }
}
