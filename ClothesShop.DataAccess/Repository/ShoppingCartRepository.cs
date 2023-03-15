using ClothesShop.DataAccess.Data;
using ClothesShop.DataAccess.Repository.IRepository;
using ClothesShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClothesShop.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;

        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCart obj)
        {
            _db.Update(obj);
        }
    }
}
