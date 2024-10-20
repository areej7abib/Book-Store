namespace Store.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles =SD.Role_Admin)]
public class CategoryController : Controller
{
    private readonly IUnitOfWork unitOfWork;

    public CategoryController(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        List<Category> categories = unitOfWork.Category.GetAll().ToList();
        return View(categories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Create(Category category)
    {
        if (category.Name.ToLower() == category.DisplayOrder.ToString())
        {
            ModelState.AddModelError("Name", "The Name and DisplayOrder Must Not be Same");
        }
        if (ModelState.IsValid)
        {
            unitOfWork.Category.Add(category);
            unitOfWork.Save();
            TempData["success"] = "The Category Created Succesfuly";
            return RedirectToAction("Index");
        }
        return View(category);
    }

    public IActionResult Edit(int? Id)
    {
        if (Id == 0 || Id == null)
        {
            return NotFound();
        }
        Category? category = unitOfWork.Category.GetById(Id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }
    [HttpPost]
    public IActionResult Edit(Category category)
    {

        if (ModelState.IsValid)
        {
            unitOfWork.Category.Update(category);
            unitOfWork.Save();
            TempData["success"] = "The Category Updated Succesfuly";
            return RedirectToAction("Index");
        }
        return View(category);
    }

    public IActionResult Delete(int? Id)
    {
        if (Id == 0 || Id == null)
        {
            return NotFound();
        }
        Category? category = unitOfWork.Category.GetById(Id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }
    [HttpPost, ActionName("Delete")]
    public IActionResult DeletePost(int? Id)
    {
        if (Id == null)
        {
            return NotFound();
        }
        Category? category = unitOfWork.Category.Get(i => i.Id == Id);
        if (category == null)
        {
            return NotFound();
        }
        unitOfWork.Category.Delete(category);
        unitOfWork.Save();
        TempData["success"] = "The Category Deleted Succesfuly";
        return RedirectToAction("Index");
    }
}
