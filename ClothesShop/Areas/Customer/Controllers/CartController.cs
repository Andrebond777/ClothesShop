using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ClothesShop.DataAccess.Repository.IRepository;
using ClothesShop.Models;
using ClothesShop.Models.ViewModels;
using ClothesShop.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Stripe;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ClothesShop.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender, 
            UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCart = _unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId==claim.Value, includeProperties: "Product,Product.Category,Product.ImagesUrl,Product.Colors").ToList()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                                                        .GetFirstOrDefault(u => u.Id == claim.Value);
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            foreach (var list in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Product.Price * list.Count);
                list.OneTypeTotal = (list.Product.Price * list.Count);
            }

            return View(ShoppingCartVM);
        }

        
        //[HttpPost]
        //[ActionName("Index")]
        //public async Task<IActionResult> IndexPOST()
        //{
        //    var claimsIdentity = (ClaimsIdentity)User.Identity;
        //    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        //    var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

        //    if (user == null)
        //    {
        //        ModelState.AddModelError(string.Empty, "Verification email is empty!");
        //    }

        //    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        //    var callbackUrl = Url.Page(
        //        "/Account/ConfirmEmail",
        //        pageHandler: null,
        //        values: new { area = "Identity", userId = user.Id, code = code },
        //        protocol: Request.Scheme);

        //    await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
        //        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        //    ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
        //    return RedirectToAction("Index");

        //}

        //public IActionResult SizeInput(int cartId, string size)
        //{
        //    var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault
        //                    (c => c.Id == cartId, includeProperties: "Product,Product.ImagesUrl");
        //    cart.Size = size;
        //    cart.Price = cart.Product.Price;
        //    _unitOfWork.Save();
        //    return RedirectToAction(nameof(Index));
        //}

        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault
                            (c => c.Id == cartId, includeProperties: "Product,Product.ImagesUrl,Product.Colors");
            cart.Count += 1;
            cart.Price = cart.Product.Price;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault
                            (c => c.Id == cartId, includeProperties: "Product,Product.ImagesUrl,Product.Colors");

            if (cart.Count == 1)
            {
                var cnt = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);                
            }
            else
            {
                 cart.Count -= 1;
                cart.Price = cart.Product.Price;
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault
                            (c => c.Id == cartId, includeProperties: "Product,Product.ImagesUrl,Product.Colors");

             var cnt = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Clear(ShoppingCartVM cartVM)
        {
            foreach (var cart in cartVM.ListCart)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
            }
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, 0);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult PlusSummary(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault
                            (c => c.Id == cartId, includeProperties: "Product,Product.ImagesUrl,Product.Colors");
            cart.Count += 1;
            cart.Price = cart.Product.Price;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Summary));
        }

        public IActionResult MinusSummary(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault
                            (c => c.Id == cartId, includeProperties: "Product,Product.ImagesUrl,Product.Colors");

            if (cart.Count == 1)
            {
                var cnt = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);
            }
            else
            {
                cart.Count -= 1;
                cart.Price = cart.Product.Price;
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Summary));
        }

        public IActionResult RemoveSummary(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault
                            (c => c.Id == cartId, includeProperties: "Product,Product.ImagesUrl,Product.Colors");

            var cnt = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);


            return RedirectToAction(nameof(Summary));
        }

        [HttpPost]
        [ActionName("Index")]
        public IActionResult IndexPOST(ShoppingCartVM CartVMObject)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM cartFromDb = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value,
                                                            includeProperties: "Product,Product.ImagesUrl,Product.Colors,Product.Category").ToList()
            };

            if (!ShoppingCartVM.ListCart.Any())
                return RedirectToAction(nameof(Index));

            for (int i = 0; i < cartFromDb.ListCart.Count; i++)
            {
                cartFromDb.ListCart[i].Size = CartVMObject.ListCart[i].Size;
                cartFromDb.ListCart[i].Color = CartVMObject.ListCart[i].Color;
            }

            //foreach (var item in cartFromDb.ListCart)
            //{
            //    var newValue = CartVMObject.ListCart.FirstOrDefault(_ => _.Id == item.Id);

            //    if(newValue?.Size != null)
            //    {
            //        item.Size = newValue.Size;
            //    }
            //}

            _unitOfWork.Save();

            return RedirectToAction("Summary");
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value,
                                                            includeProperties: "Product,Product.ImagesUrl,Product.Colors,Product.Category").ToList()
            };

            if (!ShoppingCartVM.ListCart.Any())
                return RedirectToAction(nameof(Index));

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                                                            .GetFirstOrDefault(c => c.Id == claim.Value);

            foreach (var list in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Product.Price * list.Count);
                list.OneTypeTotal = (list.Product.Price * list.Count);
            }
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;

            //ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult SummaryPost(ShoppingCartVM CartVMObject)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                                                            .GetFirstOrDefault(c => c.Id == claim.Value);

            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart
                                        .GetAll(c => c.ApplicationUserId == claim.Value,
                                        includeProperties: "Product,Product.ImagesUrl,Product.Colors").ToList();

            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();


            for (int i = 0; i < ShoppingCartVM.ListCart.Count(); i++)
            {
                ShoppingCartVM.ListCart[i].Price = ShoppingCartVM.ListCart[i].Product.Price;
                OrderDetails orderDetails = new OrderDetails()
                {
                    Size = CartVMObject.ListCart[i].Size,
                    Color = CartVMObject.ListCart[i].Color,
                    ProductId = ShoppingCartVM.ListCart[i].ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = ShoppingCartVM.ListCart[i].Price,
                    Count = ShoppingCartVM.ListCart[i].Count
                };
                ShoppingCartVM.OrderHeader.OrderTotal += orderDetails.Count * orderDetails.Price;
                _unitOfWork.OrderDetails.Add(orderDetails);
            }

            //int i = 0;
            //foreach (var item in ShoppingCartVM.ListCart)
            //{
            //    item.Price = item.Product.Price;
            //    OrderDetails orderDetails = new OrderDetails()
            //    {
            //        Size = CartVMObject.ListCart[i].Size,
            //        Color = CartVMObject.ListCart[i].Color,
            //        ProductId = item.ProductId,
            //        OrderId = ShoppingCartVM.OrderHeader.Id,
            //        Price = item.Price,
            //        Count = item.Count
            //    };
            //    ShoppingCartVM.OrderHeader.OrderTotal += orderDetails.Count * orderDetails.Price;
            //    _unitOfWork.OrderDetails.Add(orderDetails);
            //    i++;
            //}

            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, 0);

            //if (stripeToken == null)
            //{
                //order will be created for delayed payment for authroized company
                //ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            //}
            //else
            //{
            //    //process the payment
            //    var options = new ChargeCreateOptions
            //    {
            //        Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal * 100),
            //        Currency = "usd",
            //        Description = "Order ID : " + ShoppingCartVM.OrderHeader.Id,
            //        Source = stripeToken
            //    };

            //    var service = new ChargeService();
            //    Charge charge = service.Create(options);

            //    if (charge.Id == null)
            //    {
            //        ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            //    }
            //    else
            //    {
            //        ShoppingCartVM.OrderHeader.TransactionId = charge.Id;
            //    }
            //    if (charge.Status.ToLower() == "succeeded")
            //    {
            //        ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
            //        ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            //        ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
            //    }
            //}

            _unitOfWork.Save();

            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });

        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);

            return View(id);
        }
    }
}