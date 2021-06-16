using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
    public class ColorRepository : Repository<Color>, IColorRepository
    {
        private readonly ApplicationDbContext _db;

        public ColorRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Color color)
        {
            var objFromDb = _db.Colors.FirstOrDefault(s => s.Id == color.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = color.Name; 
            }
        }
    }
}
