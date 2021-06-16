using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace BulkyBook.Areas.Customer.Controllers
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

        public IActionResult Index(string categoryName, string searchString)
        {
            //Expression<Action<string>> ex = s => s.Category.Name == categoryName;
            IEnumerable<Product> productList;
            if (categoryName == null)
                productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ImagesUrl,Colors");
            else
                productList = _unitOfWork.Product.GetAll(s => s.Category.SubName == categoryName || s.Category.MainName == categoryName , includeProperties: "Category,ImagesUrl,Colors");

            if(searchString != null)
                productList = productList.Where(x => x.Title.ToLower().Contains(searchString.ToLower()));

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var count = _unitOfWork.ShoppingCart
                    .GetAll(c => c.ApplicationUserId == claim.Value)
                    .ToList().Count();

                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);
            }

            var distinctArr = _unitOfWork.Category.GetAll().ToList();

            for (int i = 0; i < distinctArr.Count-1; i++)
            {
                for (int j = i + 1; j < distinctArr.Count; j++)
                {
                    if (distinctArr[i].SubName == distinctArr[j].SubName)
                        distinctArr.RemoveAt(j);
                }
            }

            ViewBag.Categories = distinctArr;

            return View(productList);
        }

        public IActionResult Details(int id)
        {
            var productFromDb = _unitOfWork.Product.
                        GetFirstOrDefault(u => u.Id == id, "ImagesUrl,Colors,Category");
            ShoppingCart cartObj = new ShoppingCart()
            {
                Product = productFromDb,
                ProductId = productFromDb.Id
            };
            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult AddToCart(int id)
        {
            var prodFromDb = _unitOfWork.Product.
                        GetFirstOrDefault(u => u.Id == id, "ImagesUrl,Colors,Category");
            ShoppingCart CartObject = new ShoppingCart()
            {
                Product = prodFromDb,
                ProductId = prodFromDb.Id
            };

            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                //then we will add to cart
                if (User.Identity != null)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    CartObject.ApplicationUserId = claim.Value;
                }
                else
                {
                    CartObject.ApplicationUserId = null;
                }

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                    u => u.ApplicationUserId == CartObject.ApplicationUserId && u.ProductId == CartObject.ProductId
                    , includeProperties: "Product,Product.ImagesUrl,Colors,Category");

                if (cartFromDb == null)
                {
                    //no records exists in database for that product for that user
                    _unitOfWork.ShoppingCart.Add(CartObject);
                }
                else
                {
                    cartFromDb.Count += CartObject.Count;
                    //_unitOfWork.ShoppingCart.Update(cartFromDb);
                }
                _unitOfWork.Save();

                var count = _unitOfWork.ShoppingCart
                    .GetAll(c => c.ApplicationUserId == CartObject.ApplicationUserId)
                    .ToList().Count();

                //HttpContext.Session.SetObject(SD.ssShoppingCart, CartObject);
                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);

                return RedirectToAction(nameof(Details));
            }
            else
            {
                var productFromDb = _unitOfWork.Product.
                        GetFirstOrDefault(u => u.Id == CartObject.ProductId, includeProperties: "Category,ImagesUrl,Colors");
                ShoppingCart cartObj = new ShoppingCart()
                {
                    Product = productFromDb,
                    ProductId = productFromDb.Id
                };
                return RedirectToAction(nameof(Index));
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult AddToCartFromIndex(int id)
        {
            var prodFromDb = _unitOfWork.Product.
                        GetFirstOrDefault(u => u.Id == id, "ImagesUrl,Colors");
            ShoppingCart CartObject = new ShoppingCart()
            {
                Product = prodFromDb,
                ProductId = prodFromDb.Id
            };

            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                //then we will add to cart
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                    u => u.ApplicationUserId == CartObject.ApplicationUserId && u.ProductId == CartObject.ProductId
                    , includeProperties: "Product,Product.ImagesUrl,Product.Colors");

                if (cartFromDb == null)
                {
                    //no records exists in database for that product for that user
                    _unitOfWork.ShoppingCart.Add(CartObject);
                }
                else
                {
                    cartFromDb.Count += CartObject.Count;
                }
                _unitOfWork.Save();

                var count = _unitOfWork.ShoppingCart
                    .GetAll(c => c.ApplicationUserId == CartObject.ApplicationUserId)
                    .ToList().Count();

                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);

                return RedirectToAction("Index", "Cart");
                //return RedirectToAction(nameof(Index));
            }
            else
            {
                var productFromDb = _unitOfWork.Product.
                        GetFirstOrDefault(u => u.Id == CartObject.ProductId, includeProperties: "Category,ImagesUrl,Colors");
                ShoppingCart cartObj = new ShoppingCart()
                {
                    Product = productFromDb,
                    ProductId = productFromDb.Id
                };

                return RedirectToAction("Index", "Cart");
                //return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart CartObject)
        {
            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                //then we will add to cart
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                    u => u.ApplicationUserId == CartObject.ApplicationUserId && u.ProductId == CartObject.ProductId
                    && u.Size == CartObject.Size && u.Color == CartObject.Color, includeProperties: "Product,Product.ImagesUrl,Product.Colors"
                    );

                if (cartFromDb == null)
                {
                    //no records exists in database for that product for that user
                    _unitOfWork.ShoppingCart.Add(CartObject);
                }
                else
                {
                    cartFromDb.Count += CartObject.Count;
                    //_unitOfWork.ShoppingCart.Update(cartFromDb);
                }
                _unitOfWork.Save();

                var count = _unitOfWork.ShoppingCart
                    .GetAll(c => c.ApplicationUserId == CartObject.ApplicationUserId)
                    .ToList().Count();

                //HttpContext.Session.SetObject(SD.ssShoppingCart, CartObject);
                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);

                return RedirectToAction(nameof(Details));
            }
            else
            {
                var productFromDb = _unitOfWork.Product.
                        GetFirstOrDefault(u => u.Id == CartObject.ProductId, includeProperties: "Category,ImagesUrl,Colors");
                ShoppingCart cartObj = new ShoppingCart()
                {
                    Product = productFromDb,
                    ProductId = productFromDb.Id
                };
                return View(cartObj);
            }    
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
