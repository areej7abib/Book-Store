using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Security.Claims;

namespace Store.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public OrderViewModel order { get; set; }

        public OrderController(IUnitOfWork iunitOfWork)
        {
            this.unitOfWork = iunitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            order = new()
            {
                orderDetail = unitOfWork.OrderDetail.GetAll(e => e.OrderHeaderId == orderId, IncludeProperities: "Product"),
                orderHeader = unitOfWork.OrderHeader.Get(e => e.Id == orderId, IncludeProperities: "ApplicationUser")
            };
            return View(order);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            OrderHeader orderheaderfromDb = unitOfWork.OrderHeader.Get(u => u.Id == order.orderHeader.Id);
            orderheaderfromDb.Name = order.orderHeader.Name;
            orderheaderfromDb.State = order.orderHeader.State;
            orderheaderfromDb.City = order.orderHeader.City;
            orderheaderfromDb.PhoneNumber = order.orderHeader.PhoneNumber;
            orderheaderfromDb.StreetAddress = order.orderHeader.StreetAddress;
            orderheaderfromDb.PostalCode = order.orderHeader.PostalCode;
            if (!string.IsNullOrEmpty(order.orderHeader.Carrier))
                orderheaderfromDb.Carrier = order.orderHeader.Carrier;
            if (!string.IsNullOrEmpty(order.orderHeader.TrackingNumber))
                orderheaderfromDb.TrackingNumber = order.orderHeader.TrackingNumber;
            unitOfWork.OrderHeader.Update(orderheaderfromDb);
            unitOfWork.Save();
            TempData["success"] = "Order Detail updated succesfuly";
            return RedirectToAction(nameof(Details), new { orderId = order.orderHeader.Id });

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            unitOfWork.OrderHeader.UpdateStatus(order.orderHeader.Id, SD.StatusInProcess, null);
            unitOfWork.Save();
            TempData["success"] = "Order Detail updated succesfuly";
            return RedirectToAction(nameof(Details), new { orderId = order.orderHeader.Id });

        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderheaderfromDb = unitOfWork.OrderHeader.Get(e => e.Id == order.orderHeader.Id);
            orderheaderfromDb.Carrier = order.orderHeader.Carrier;
            orderheaderfromDb.TrackingNumber = order.orderHeader.TrackingNumber;
            orderheaderfromDb.OrderStatus = SD.StatusShipped;
            orderheaderfromDb.ShippingDate = DateTime.Now;
            if (orderheaderfromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
                orderheaderfromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            unitOfWork.OrderHeader.Update(orderheaderfromDb);
            unitOfWork.Save();
            TempData["success"] = "Order is shipped succesfuly";
            return RedirectToAction(nameof(Details), new { orderId = order.orderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {

            var orderHeader = unitOfWork.OrderHeader.Get(u => u.Id == order.orderHeader.Id);

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = order.orderHeader.Id });

        }
        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            order.orderHeader = unitOfWork.OrderHeader
                .Get(u => u.Id == order.orderHeader.Id, IncludeProperities: "ApplicationUser");
            order.orderDetail = unitOfWork.OrderDetail
                .GetAll(u => u.OrderHeaderId == order.orderHeader.Id, IncludeProperities: "Product");

            //stripe logic
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={order.orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={order.orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in order.orderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            unitOfWork.OrderHeader.UpdateStripePaymentId(order.orderHeader.Id, session.Id, session.PaymentIntentId);
            unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {

            OrderHeader orderHeader = unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    unitOfWork.Save();
                }
            }

            return View(orderHeaderId);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<OrderHeader>? orderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = unitOfWork.OrderHeader.GetAll(IncludeProperities: "ApplicationUser").ToList();
            }
            else
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderHeaders = unitOfWork.OrderHeader
                        .GetAll(e => e.ApplicationUserId == userId, IncludeProperities: "ApplicationUser").ToList();
            }

            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
