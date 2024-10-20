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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationContext dB;

        public OrderHeaderRepository(ApplicationContext _DB):base(_DB)
        {
            dB = _DB;
        }

        public void Update(OrderHeader order)
        {
           dB.orderHeaders.Update(order);
        }

		public void UpdateStatus(int id, string orderStaus, string? paymentStatus)
		{
			OrderHeader? orderheaderDb = dB.orderHeaders.FirstOrDefault(e=>e.Id == id);
            if (orderheaderDb != null)
            {
                orderheaderDb.OrderStatus = orderStaus;
                if(!string.IsNullOrEmpty(paymentStatus))
                    orderheaderDb.PaymentStatus = paymentStatus;
            }
		}

		public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
		{
			OrderHeader? orderheaderDb = dB.orderHeaders.FirstOrDefault(e=>e.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderheaderDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderheaderDb.PaymentIntentId = paymentIntentId;
                orderheaderDb.PaymentDate = DateTime.Now;
            }
		}
	}
}
