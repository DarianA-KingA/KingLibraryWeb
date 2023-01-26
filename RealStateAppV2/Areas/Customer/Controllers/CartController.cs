using KingLibraryWeb.DataAccess.Repository.IRepository;
using KingLibraryWeb.Models;
using KingLibraryWeb.Models.ViewModels;
using KingLibraryWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace KingLibraryWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize()]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppinCartViewModel ShoppingCartVm { get; set; }
        public int OrderTotal { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //[AllowAnonymous]//this allows not authorized user get access to this action
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVm = new ShoppinCartViewModel()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            foreach (var cart in ShoppingCartVm.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVm);
        }
        public IActionResult Summary()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVm = new ShoppinCartViewModel()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            ShoppingCartVm.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            ShoppingCartVm.OrderHeader.Name = ShoppingCartVm.OrderHeader.ApplicationUser.Name;
            ShoppingCartVm.OrderHeader.PhoneNumber = ShoppingCartVm.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVm.OrderHeader.StreetAddress = ShoppingCartVm.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVm.OrderHeader.City = ShoppingCartVm.OrderHeader.ApplicationUser.City;
            ShoppingCartVm.OrderHeader.State = ShoppingCartVm.OrderHeader.ApplicationUser.State;
            ShoppingCartVm.OrderHeader.PostalCode = ShoppingCartVm.OrderHeader.ApplicationUser.PostalCode;
            foreach (var cart in ShoppingCartVm.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVm);
        }
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST()
        {

             var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVm.ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product");

           
            ShoppingCartVm.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVm.OrderHeader.ApplicationUserId = claim.Value;

            foreach (var cart in ShoppingCartVm.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            if (user.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVm.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVm.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else 
            {
                ShoppingCartVm.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVm.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            _unitOfWork.OrderHeader.Add(ShoppingCartVm.OrderHeader);
            _unitOfWork.Save();
            foreach (var cart in ShoppingCartVm.ListCart)
            {
                OrderDetail orderDetail = new() { 
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVm.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
            if (user.CompanyId.GetValueOrDefault() == 0)
            {
                //
                #region Payment Section stripe setting
                var domain = "https://localhost:7224/";
                var options = new SessionCreateOptions//make sure that de using is stripe.checkout and not billing!!!
                {
                    CancelUrl = domain + "customer/cart/index",
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVm.OrderHeader.Id}",
                    LineItems = new List<SessionLineItemOptions>(),
                    PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },

                    Mode = "payment",
                };
                foreach (var item in ShoppingCartVm.ListCart)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            },         
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVm.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
                #endregion
                //
            }
            else 
            {
                return RedirectToAction("OrderConfirmation","Cart",new { id = ShoppingCartVm.OrderHeader.Id});
            }
        }
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);//here we get the payment intentId
                //check the stripe status
                _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeader.Id, session.Id, session.PaymentIntentId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            HttpContext.Session.Clear();//Limpiamos la sesion para que el count del carrito vuelva a 0
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }
        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count()-1;
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }
            else 
            {
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            var count = _unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.SessionCart, count);
            return RedirectToAction(nameof(Index));
        }
        private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else if (quantity <= 100)
            {
                return price50;
            }
            return price100;
        }
    }
}
