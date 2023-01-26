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
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment; 
        }
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Upsert(int? id)
        {
            ProductViewModel productVm = new() { 
                product = new(),
                CategoryList =_unitOfWork.Category.GetAll().Select(i=> new SelectListItem { 
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem {
                    Text= i.Name,
                    Value = i.Id.ToString()
                }),
            };
;
            if (id == null || id == 0)
            {
                //create product
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(productVm);
            }
            else
            {
                productVm.product = _unitOfWork.Product.GetFirstOrDefault(p=>p.Id == id);
                return View(productVm);

                //edit product
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel obj, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                var wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"Images\Products");
                    var extension = Path.GetExtension(file.FileName);

                    if (obj.product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create)) {
                        file.CopyTo(fileStreams);
                    }
                    obj.product.ImageUrl = @"\Images\products\"+fileName+extension;
                }
                if (obj.product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.product);
                }
                else
                {
                    _unitOfWork.Product.Update(obj.product);
                }
                
                _unitOfWork.Save();
                TempData["success"] = "Product created succesfully";
                return RedirectToAction("Index");
            }
            obj.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            obj.CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            return View(obj);
        }
        #region API CALL
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CovertType");//make sure that the include macht with the navigationProperty name, it is case sensitive
            return Json(new { data = productList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var Product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
            if (Product == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            var wwwRootPath = _hostEnvironment.WebRootPath;
            var oldImagePath = Path.Combine(wwwRootPath, Product.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.Product.Remove(Product);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion
    }
}
