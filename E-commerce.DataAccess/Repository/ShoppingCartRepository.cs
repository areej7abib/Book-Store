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
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationContext context;

        public ShoppingCartRepository(ApplicationContext context) : base(context)
        {
            this.context = context;
        }

        public void Update(ShoppingCart shoppingCart)
        {
           context.shoppingCarts.Update(shoppingCart);
        }
    }
}
