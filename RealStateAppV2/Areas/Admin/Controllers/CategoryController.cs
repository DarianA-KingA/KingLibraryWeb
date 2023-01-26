using Microsoft.AspNetCore.Mvc;
using KingLibraryWeb.DataAccess.Repository.IRepository;
using KingLibraryWeb.Models;
using Microsoft.AspNetCore.Authorization;
using KingLibraryWeb.Utility;

namespace KingLibraryWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> categoryList = _unitOfWork.Category.GetAll();
            return View(categoryList);
        }
        public IActionResult Create()
        {
            return View(new Category());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category vm)
        {
            if (vm.Name == vm.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "No puede haber match");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(vm);
                _unitOfWork.Save();
                TempData["success"] = "Category created succesfully";
                return RedirectToAction("Index");
            }
            return View(vm);
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryById = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            if (categoryById == null)
            {
                return NotFound();
            }
            return View(categoryById);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category vm)
        {
            if (vm.Name == vm.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "No puede haber match");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(vm);
                _unitOfWork.Save();
                TempData["success"] = "Category updated succesfully";
                return RedirectToAction("Index");
            }
            return View(vm);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryById = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            if (categoryById == null)
            {
                return NotFound();
            }
            return View(categoryById);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var category = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted succesfully";
            return RedirectToAction("Index");
        }
    }
}
