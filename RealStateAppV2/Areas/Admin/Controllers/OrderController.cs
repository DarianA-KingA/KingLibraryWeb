using KingLibraryWeb.DataAccess.Repository.IRepository;
using KingLibraryWeb.Models;
using KingLibraryWeb.Models.ViewModels;
using KingLibraryWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace KingLibraryWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderViewModel orderVm { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork= unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            orderVm = new() { 
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(u=>u.OrderId == orderId, includeProperties: "Product")
            };
            return View(orderVm);
        }
        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_Pay_Now()
        {
            orderVm.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVm.OrderHeader.Id, includeProperties: "ApplicationUser");
            orderVm.OrderDetails = _unitOfWork.OrderDetail.GetAll(u=>u.OrderId ==orderVm.OrderHeader.Id, includeProperties: "Product");

            #region Payment Section stripe setting
            var domain = "https://localhost:7224/";
            var options = new SessionCreateOptions
            {
                CancelUrl = domain + $"admin/order/details?orderId={orderVm.OrderHeader.Id}",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?OrderHeaderid={orderVm.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },

                Mode = "payment",
            };
            foreach (var item in orderVm.OrderDetails)
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
            _unitOfWork.OrderHeader.UpdateStripePaymentId(orderVm.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
            #endregion
        }
        public IActionResult PaymentConfirmation(int OrderHeaderid)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderHeaderid);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);//here we get the payment intentId
                //check the stripe status
                _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeader.Id, session.Id, session.PaymentIntentId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStatus(OrderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            return View(OrderHeaderid);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVm.OrderHeader.Id, tracked:false);
            orderHeaderFromDb.Name = orderVm.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVm.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderVm.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = orderVm.OrderHeader.City;
            orderHeaderFromDb.PostalCode = orderVm.OrderHeader.PostalCode;
            orderHeaderFromDb.State = orderVm.OrderHeader.State;
            if (orderVm.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = orderVm.OrderHeader.Carrier;
            }
            if (orderVm.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = orderVm.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updates Successfully";
            return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDb.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderVm.OrderHeader.Id, SD.StatusProccessing );
            _unitOfWork.Save();
            TempData["Success"] = "Order Status Updates Successfully";
            return RedirectToAction("Details", "Order", new { orderId = orderVm.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVm.OrderHeader.Id, tracked: false);
            orderHeaderFromDb.TrackingNumber = orderVm.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = orderVm.OrderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            orderHeaderFromDb.ShippingDate = DateTime.Now;
            if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully";
            return RedirectToAction("Details", "Order", new { orderId = orderVm.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVm.OrderHeader.Id, tracked: false);
            if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntentId,
                    //Amount = Convert.ToInt32(orderHeaderFromDb.OrderTotal * 100) opcional, para indicar el monto que se va a reembolsar, se pone el precio *100 para obtener el precio real
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully";
            return RedirectToAction("Details", "Order", new { orderId = orderVm.OrderHeader.Id });
        }
        #region API CALL
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee)){
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId == claim.Value,includeProperties: "ApplicationUser");
            }
            //orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");//make sure that the include macht with the navigationProperty name, it is case sensitive
            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusPending);
                    break;
                case "inproccess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusProccessing);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
               case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
