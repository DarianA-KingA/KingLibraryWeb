using Microsoft.AspNetCore.Mvc;
using KingLibraryWeb.DataAccess.Repository.IRepository;
using KingLibraryWeb.Models;
using KingLibraryWeb.Utility;
using Microsoft.AspNetCore.Authorization;

namespace KingLibraryWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<CoverType> TipoVentaList = _unitOfWork.CoverType.GetAll();
            return View(TipoVentaList);
        }
        public IActionResult Create()
        {
            return View(new CoverType());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType vm)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Add(vm);
                _unitOfWork.Save();
                TempData["success"] = "Cover Type created succesfully";
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
            var TipoVentaById = _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);
            if (TipoVentaById == null)
            {
                return NotFound();
            }
            return View(TipoVentaById);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType vm)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(vm);
                _unitOfWork.Save();
                TempData["success"] = "Cover Type updated succesfully";
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
            var TipoVentaById = _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);
            if (TipoVentaById == null)
            {
                return NotFound();
            }
            return View(TipoVentaById);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var TipoVenta = _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);
            if (TipoVenta == null)
            {
                return NotFound();
            }
            _unitOfWork.CoverType.Remove(TipoVenta);
            _unitOfWork.Save();
            TempData["success"] = "Cover Type deleted succesfully";
            return RedirectToAction("Index");
        }
    }
}
