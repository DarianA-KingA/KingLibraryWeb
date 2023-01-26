using Microsoft.AspNetCore.Mvc;
using KingLibraryWeb.Models;
using System.Diagnostics;
using KingLibraryWeb.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using KingLibraryWeb.Utility;

namespace KingLibraryWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CovertType");
            return View(productList);
        }
        public IActionResult Details(int ProductId)
        {
            ShoppingCart cartPbj = new() { 
                ProductId = ProductId,
                Product = _unitOfWork.Product.GetFirstOrDefault(u=>u.Id == ProductId,includeProperties: "Category,CovertType"),
                Count = 1
            }; 
            return View(cartPbj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;
            ShoppingCart CartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);
            if (CartFromDb == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count());
            }
            else
            {

                _unitOfWork.ShoppingCart.IncrementCount(CartFromDb, shoppingCart.Count);
                _unitOfWork.Save();
            }
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
}