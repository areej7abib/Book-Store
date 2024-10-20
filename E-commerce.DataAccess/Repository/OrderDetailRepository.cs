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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationContext dB;

        public OrderDetailRepository(ApplicationContext _DB):base(_DB)
        {
            dB = _DB;
        }

        public void Update(OrderDetail order)
        {
           dB.orderDetails.Update(order);
        }

      

    }
}
