using System.Security.Claims;

namespace Store.Areas.Customer.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork unitOfWork;

    public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
    {
        _logger = logger;
        this.unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> products = unitOfWork.Product.GetAll(IncludeProperities: "Category");
        return View(products);
    }
    public IActionResult Details(int productId)
    {
        ShoppingCart cart = new()
        {
            Product = unitOfWork.Product.Get(u => u.Id == productId, IncludeProperities: "Category"),
            Count = 1,
            ProductId = productId
        };
        return View(cart);
    }
    [HttpPost]
    [Authorize]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        shoppingCart.ApplicationUserId = userId;

        ShoppingCart cartFromDb = unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId &&
        u.ProductId == shoppingCart.ProductId);

        if (cartFromDb != null)
        {
            //shopping cart exists
            cartFromDb.Count += shoppingCart.Count;
            unitOfWork.ShoppingCart.Update(cartFromDb);
            unitOfWork.Save();
        }
        else
        {
            //add cart record
            unitOfWork.ShoppingCart.Add(shoppingCart);
            unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.SessionCart,
            unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
        }
        TempData["success"] = "Cart updated successfully";




        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
