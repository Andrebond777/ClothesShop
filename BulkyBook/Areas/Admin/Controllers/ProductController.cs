using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace BulkyBook.Areas.Admin.Controllers
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

        public IActionResult AddColors(int? productId)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == productId, "Colors");
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddColors(Product product)
        {
            var productFromDb = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == product.Id, "Colors");
            productFromDb.Colors = product.Colors;

            _unitOfWork.Product.Update(productFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteImages(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Index));
            }

            IEnumerable<Category> CatList = _unitOfWork.Category.GetAll();
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                MainCategoryList = CatList.Select(i => new SelectListItem
                {
                    Text = i.MainName,
                    Value = i.Id.ToString()
                }),
                SubCategoryList = CatList.Select(i => new SelectListItem
                {
                    Text = i.SubName,
                    Value = i.Id.ToString()
                }),
            };

            productVM.Product = _unitOfWork.Product.GetFirstOrDefault(_ => _.Id == id, "ImagesUrl,Colors");
            if (productVM.Product == null)
            {
                return NotFound();
            }

            productVM.Product.ImagesUrl.RemoveAll(i => true);
            _unitOfWork.Product.Update(productVM.Product);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Upsert(int? id)
        {
            IEnumerable<Category> CatList = _unitOfWork.Category.GetAll();
            ProductVM productVM = new ProductVM()
            {
                Product=new Product(),
                MainCategoryList = CatList.Select(i=> new SelectListItem { 
                    Text = i.MainName,
                    Value = i.Id.ToString()
                }),
                SubCategoryList = CatList.Select(i => new SelectListItem
                {
                    Text = i.SubName,
                    Value = i.Id.ToString()
                }),
            };

            if (id == null)
            {
                //this is for create
                return View(productVM);
            }

            productVM.Product = _unitOfWork.Product.GetFirstOrDefault(_ => _.Id == id, "ImagesUrl,Colors");
            if (productVM.Product == null)
            {
                return NotFound();
            }
            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM, int colorsQuantity)
        {

            if (ModelState.IsValid)
            {
                productVM.Product.Colors = new List<Color>(colorsQuantity);
                for (int i = 0; i < colorsQuantity; i++)
                {
                    productVM.Product.Colors.Add(new Color());
                }

                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    if (productVM.Product.ImagesUrl == null)
                    {
                        productVM.Product.ImagesUrl = new List<FilePath>(files.Count);
                    }

                    for (int i = 0; i < productVM.Product.ImagesUrl.Capacity; i++)
                    {
                        var filePath = new FilePath();
                        string fileName = Guid.NewGuid().ToString();
                        var uploads = Path.Combine(webRootPath, @"images\products");
                        var extenstion = Path.GetExtension(files[i].FileName);
                        if (filePath.Path != null)
                        {
                            //this is an edit and we need to remove old image
                            var imagePath = Path.Combine(webRootPath, filePath.Path.TrimStart('\\'));
                            if (System.IO.File.Exists(imagePath))
                            {
                                System.IO.File.Delete(imagePath);
                            }
                        } 
                        using (var filesStreams = new FileStream(Path.Combine(uploads,fileName+extenstion),FileMode.Create))
                        {
                            files[i].CopyTo(filesStreams);
                        }
                        filePath.Path = @"\images\products\" + fileName + extenstion;
                        productVM.Product.ImagesUrl.Add(filePath);
                    }
                }
                else
                {
                    //update when they do not change the image
                    if(productVM.Product.Id != 0)
                    {
                        Product objFromDb = _unitOfWork.Product.Get(productVM.Product.Id);
                        productVM.Product.ImagesUrl = objFromDb.ImagesUrl;
                    }
                }


                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);

                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(AddColors), new { productId = productVM.Product.Id });
            }
            else
            {
                IEnumerable<Category> CatList = _unitOfWork.Category.GetAll();
                productVM.MainCategoryList = CatList.Select(i => new SelectListItem
                {
                    Text = i.MainName,
                    Value = i.Id.ToString()
                });
                productVM.SubCategoryList = CatList.Select(i => new SelectListItem
                {
                    Text = i.SubName,
                    Value = i.Id.ToString()
                });

                if (productVM.Product.Id != 0)
                {
                    productVM.Product = _unitOfWork.Product.Get(productVM.Product.Id);
                }
            }
            return View(productVM);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Product.GetAll(includeProperties:"Category");
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Product.GetFirstOrDefault(_ => _.Id == id, "ImagesUrl,Colors");
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            string webRootPath = _hostEnvironment.WebRootPath;
            for (int i = 0; i < objFromDb.ImagesUrl.Count(); i++)
            {
                var imagePath = Path.Combine(webRootPath, objFromDb.ImagesUrl[i].Path.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                    _unitOfWork.FilePath.Remove(objFromDb.ImagesUrl[i]);
                }
            }

            for (int i = 0; i < objFromDb.Colors.Count(); i++)
            {
                _unitOfWork.Color.Remove(objFromDb.Colors[i]);
            }

                _unitOfWork.Product.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }

        #endregion
    }
}