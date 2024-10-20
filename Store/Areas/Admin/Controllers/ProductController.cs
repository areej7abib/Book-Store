namespace Store.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles =SD.Role_Admin)]
public class ProductController : Controller
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IWebHostEnvironment webHostEnvironment;

    public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment)
    {
        this.unitOfWork = unitOfWork;
        this.webHostEnvironment = webHostEnvironment;
    }
    public IActionResult Index()
    {
        List<Product>? Products = unitOfWork.Product.GetAll(IncludeProperities: "Category").ToList();
        
        return View(Products);
    }
    [HttpGet]
    public IActionResult Upsert(int? Id)
    {
        ProductViewModel productViewModel = new ProductViewModel();
        productViewModel.Product = new Product();
        productViewModel.CategoryList = unitOfWork.Category.GetAll().Select(e => new SelectListItem
        {
            Text = e.Name,
            Value = e.Id.ToString()
        });
        if(Id==null || Id == 0)
        {
            //create
            return View(productViewModel);
        }
        else
        {
            //update
            productViewModel.Product = unitOfWork.Product.GetById(Id);
            return View(productViewModel);
        }
        
    }
    [HttpPost]
    public IActionResult Upsert(ProductViewModel productViewModel,IFormFile? File)
    {
        if (ModelState.IsValid)
        {
            string Rootfile = webHostEnvironment.WebRootPath;
            if (File != null)
            {
                string Filename = Guid.NewGuid().ToString()+Path.GetExtension(File.FileName);
                string? productpath= Path.Combine(Rootfile, @"Photos\Product");
             
                if(productViewModel.Product.ImageUrl != null)
                {
                    var OldImgPath = Path.Combine(Rootfile,productViewModel.Product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(OldImgPath))
                    {
                        System.IO.File.Delete(OldImgPath);
                    }
                }

                using (FileStream fileStream = new FileStream(Path.Combine(productpath, Filename), FileMode.Create))
                {
                    File.CopyTo(fileStream);
                }

                productViewModel.Product.ImageUrl = @"\photos\product\"+Filename;
            }


            if(productViewModel.Product.CategoryId == 0)
            {
                ModelState.AddModelError("productViewModel.Product.CategoryId", "Please Select a Category");
                return View(productViewModel);
            }
            if(productViewModel.Product.Id == 0)
            {
                unitOfWork.Product.Add(productViewModel.Product);
                TempData["success"] = "The Category Created Succesfuly";
            }
            else
            {
                unitOfWork.Product.update(productViewModel.Product);
                TempData["success"] = "The Category Updated Succesfuly";
            }              
            unitOfWork.Save();
            return RedirectToAction("Index");
        }
        productViewModel.CategoryList = unitOfWork.Category.GetAll().Select(e => new SelectListItem{
            Text = e.Name,
            Value = e.Id.ToString()
        });
        return View(productViewModel);
    }

    #region API CALLS
    [HttpGet]
    public IActionResult GetAll()
    {
        List<Product>? Products = unitOfWork.Product.GetAll(IncludeProperities: "Category").ToList();
        return Json(new { data = Products });
    }
    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        Product? productToBeDeleted = unitOfWork.Product.Get(u => u.Id == id);
        if (productToBeDeleted == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }

        string productPath = @"images\products\product-" + id;
        string finalPath = Path.Combine(webHostEnvironment.WebRootPath, productPath);

        if (Directory.Exists(finalPath))
        {
            string[] filePaths = Directory.GetFiles(finalPath);
            foreach (string filePath in filePaths)
            {
                System.IO.File.Delete(filePath);
            }

            Directory.Delete(finalPath);
        }


        unitOfWork.Product.Delete(productToBeDeleted);
        unitOfWork.Save();

        return Json(new { success = true, message = "Delete Successful" });
    }

    #endregion

}
