using Microsoft.AspNetCore.Hosting;

namespace Store.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class CompanyController : Controller
{
    private readonly IUnitOfWork unitOfWork;

    public CompanyController(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }
    public IActionResult Index()
    {
        List<Company> companies = unitOfWork.Company.GetAll().ToList();
        return View(companies);
    }
    public IActionResult Upsert(int? id)
    {
        if (id == null || id == 0)
        {
            return View(new Company());
        }
        else
        {
            Company company = unitOfWork.Company.GetById(id);
            return View(company);
        }
    }
    [HttpPost]
    public IActionResult Upsert(Company company)
    {
        if (ModelState.IsValid)
        {
            if (company.Id == 0)
            {
                unitOfWork.Company.Add(company);
                unitOfWork.Save();
            }
            else
            {
                unitOfWork.Company.Update(company);
                unitOfWork.Save();
            }
            return RedirectToAction("Index");
        }
        return View(company);
    }
    public IActionResult Delete(int? Id)
    {
        if (Id == 0 || Id == null)
        {
            return NotFound();
        }
        Company? company = unitOfWork.Company.GetById(Id);
        if (company == null)
        {
            return NotFound();
        }
        return View(company);
    }
    [HttpPost, ActionName("Delete")]
    public IActionResult DeletePost(int? Id)
    {
        if (Id == null)
        {
            return NotFound();
        }
        Company? company = unitOfWork.Company.Get(i => i.Id == Id);
        if (company == null)
        {
            return NotFound();
        }
        unitOfWork.Company.Delete(company);
        unitOfWork.Save();
        TempData["success"] = "The Company Deleted Succesfuly";
        return RedirectToAction("Index");
    }

}
