using ClothesShop.DataAccess.Data;
using ClothesShop.DataAccess.Repository.IRepository;
using ClothesShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClothesShop.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            var objFromDb = _db.Products.FirstOrDefault(s => s.Id == product.Id);
            if (objFromDb != null)
            {
                //foreach (var url in product.ImagesUrl)
                //{
                //    if (url != null)
                //    {
                //        objFromDb.ImageUrl = url;
                //    }
                //}
                if (product.ImagesUrl != null)
                {
                    objFromDb.ImagesUrl = product.ImagesUrl;
                }
                objFromDb.Price = product.Price;
                objFromDb.Title = product.Title;
                objFromDb.Description = product.Description;
                objFromDb.CategoryId = product.CategoryId;        
            }
        }
    }
}
