using Microsoft.AspNetCore.Mvc;
using KingLibraryWeb.DataAccess.Repository.IRepository;
using KingLibraryWeb.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using KingLibraryWeb.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using KingLibraryWeb.Utility;
using Microsoft.AspNetCore.Authorization;

namespace KingLibraryWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Upsert(int? id)
        {
            Company company = new();
;
            if (id == null || id == 0)
            {
                return View(company);
            }
            else
            {
                company = _unitOfWork.Company.GetFirstOrDefault(p=>p.Id == id);
                return View(company);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {

            if (ModelState.IsValid)
            {

                if (obj.Id == 0)
                {
                    _unitOfWork.Company.Add(obj);
                    TempData["success"] = "Product created succesfully";

                }
                else
                {
                    _unitOfWork.Company.Update(obj);
                    TempData["success"] = "Product updated succesfully";

                }

                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #region API CALL
        [HttpGet]
        public IActionResult GetAll()
        {
            var companiesList = _unitOfWork.Company.GetAll();
            return Json(new { data = companiesList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var Product = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);
            if (Product == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Company.Remove(Product);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion
    }
}
