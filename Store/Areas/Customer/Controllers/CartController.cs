using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace Store.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork unitOfWork;
		[BindProperty]
		public ShoppingCartViewModel ShoppingCartViewModel { get; set; }

		public CartController(IUnitOfWork unitOfWork)
		{
			this.unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			string? userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
			ShoppingCartViewModel = new ShoppingCartViewModel()
			{
				ShoppingCartList = unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == userId,
				IncludeProperities: "Product"),
				orderHeader = new OrderHeader()
			};

			foreach (var cart in ShoppingCartViewModel.ShoppingCartList)
			{
				cart.Price = GetTheOrderTotal(cart);
				ShoppingCartViewModel.orderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			return View(ShoppingCartViewModel);
		}
		public IActionResult Summary()
		{
			string? userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
			ShoppingCartViewModel = new ShoppingCartViewModel()
			{
				ShoppingCartList = unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == userId,
				IncludeProperities: "Product"),
				orderHeader = new OrderHeader()
			};

			ShoppingCartViewModel.orderHeader.ApplicationUser = unitOfWork.Application.Get(e => e.Id == userId);

			ShoppingCartViewModel.orderHeader.Name = ShoppingCartViewModel.orderHeader.ApplicationUser.Name;
			ShoppingCartViewModel.orderHeader.City = ShoppingCartViewModel.orderHeader.ApplicationUser.City;
			ShoppingCartViewModel.orderHeader.PostalCode = ShoppingCartViewModel.orderHeader.ApplicationUser.PostalCode;
			ShoppingCartViewModel.orderHeader.State = ShoppingCartViewModel.orderHeader.ApplicationUser.State;
			ShoppingCartViewModel.orderHeader.StreetAddress = ShoppingCartViewModel.orderHeader.ApplicationUser.StreetAddress;
			ShoppingCartViewModel.orderHeader.PhoneNumber = ShoppingCartViewModel.orderHeader.ApplicationUser.PhoneNumber;

			foreach (ShoppingCart cart in ShoppingCartViewModel.ShoppingCartList)
			{
				cart.Price = GetTheOrderTotal(cart);
				ShoppingCartViewModel.orderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			return View(ShoppingCartViewModel);
		}
		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			string? userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
			ShoppingCartViewModel.ShoppingCartList = unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == userId,
				IncludeProperities: "Product");

			ShoppingCartViewModel.orderHeader.OrderDate = DateTime.Now;
			ShoppingCartViewModel.orderHeader.ApplicationUserId = userId;

			ApplicationUser applicationUser = unitOfWork.Application.Get(e => e.Id == userId);

			foreach (ShoppingCart cart in ShoppingCartViewModel.ShoppingCartList)
			{
				cart.Price = GetTheOrderTotal(cart);
				ShoppingCartViewModel.orderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{   //regular customer
				ShoppingCartViewModel.orderHeader.OrderStatus = SD.StatusPending;
				ShoppingCartViewModel.orderHeader.PaymentStatus = SD.PaymentStatusPending;
			}
			else
			{   //company customer and he will have 30 day to pay
				ShoppingCartViewModel.orderHeader.OrderStatus = SD.StatusApproved;
				ShoppingCartViewModel.orderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
			}

			unitOfWork.OrderHeader.Add(ShoppingCartViewModel.orderHeader);
			unitOfWork.Save();

			foreach (var item in ShoppingCartViewModel.ShoppingCartList)
			{
				OrderDetail orderDetail = new OrderDetail()
				{
					ProductId = item.ProductId,
					Count = item.Count,
					Price = item.Price,
					OrderHeaderId = ShoppingCartViewModel.orderHeader.Id
				};
				unitOfWork.OrderDetail.Add(orderDetail);
				unitOfWork.Save();
			}

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				//regular customer and we need to capture payment
				//stripe logic
				var url = "https://localhost:44350/";
				var options = new SessionCreateOptions
				{
					SuccessUrl = url + $"customer/cart/OrderConfirmation/{ShoppingCartViewModel.orderHeader.Id}",
					CancelUrl = url + "customer/cart/index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

				foreach (var item in ShoppingCartViewModel.ShoppingCartList)
				{
					var sessionlineitemlist = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price * 100),  // $20.50 => 2050
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Title
							}

						},
						Quantity = item.Count
					};
					options.LineItems.Add(sessionlineitemlist);
				}

				var service = new SessionService();
				Session? session = service.Create(options);

				unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartViewModel.orderHeader.Id, session.Id, session.PaymentIntentId);
				unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);

			}

			return RedirectToAction(nameof(OrderConfirmation), new { Id = ShoppingCartViewModel.orderHeader.Id});
		}

		public IActionResult OrderConfirmation(int Id)
		{
			OrderHeader orderHeader = unitOfWork.OrderHeader.Get(e => e.Id == Id);
			if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
			{       //this is a ccustomer
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);
				if (session.PaymentStatus.ToLower() == "paid")
				{
					unitOfWork.OrderHeader.UpdateStripePaymentId(Id, session.Id, session.PaymentIntentId);
					unitOfWork.OrderHeader.UpdateStatus(Id, SD.StatusApproved, SD.PaymentStatusApproved);
					unitOfWork.Save();
				}
			}
            HttpContext.Session.Clear();
            List<ShoppingCart> shoppingCarts = unitOfWork.ShoppingCart
				.GetAll(e => e.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
			unitOfWork.ShoppingCart.DeleteRange(shoppingCarts);
			unitOfWork.Save();
			return View(Id);
		}

		public IActionResult Plus(int cartId)
		{
			var Cartformdb = unitOfWork.ShoppingCart.Get(e => e.Id == cartId);
			Cartformdb.Count += 1;
			unitOfWork.ShoppingCart.Update(Cartformdb);
			unitOfWork.Save();
			return RedirectToAction("Index");
		}
		public IActionResult Minus(int cartId)
		{
			var Cartformdb = unitOfWork.ShoppingCart.Get(e => e.Id == cartId, tracked: true);
			if (Cartformdb.Count <= 1)
			{
				unitOfWork.ShoppingCart.Delete(Cartformdb);
                HttpContext.Session.SetInt32(SD.SessionCart, unitOfWork.ShoppingCart
                .GetAll(e => e.ApplicationUserId == Cartformdb.ApplicationUserId).Count());
            }
			else
			{
				Cartformdb.Count -= 1;
				unitOfWork.ShoppingCart.Update(Cartformdb);
			}
			unitOfWork.Save();
			return RedirectToAction("Index");
		}
		public IActionResult Remove(int cartId)
		{
			var Cartformdb = unitOfWork.ShoppingCart.Get(e => e.Id == cartId,tracked:true);
			unitOfWork.ShoppingCart.Delete(Cartformdb);
			HttpContext.Session.SetInt32(SD.SessionCart, unitOfWork.ShoppingCart
				.GetAll(e => e.ApplicationUserId == Cartformdb.ApplicationUserId).Count());
			unitOfWork.Save();
			return RedirectToAction("Index");
		}

		private double GetTheOrderTotal(ShoppingCart cart)
		{
			if (cart.Count <= 50)
			{
				return cart.Product.Price;
			}
			else
			{
				if (cart.Count <= 100)
				{
					return cart.Product.Price50;
				}
				else
				{
					return cart.Product.Price100;
				}
			}
		}
	}
}
