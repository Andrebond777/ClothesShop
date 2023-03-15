using ClothesShop.DataAccess.Data;
using ClothesShop.DataAccess.Repository.IRepository;
using ClothesShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClothesShop.DataAccess.Repository
{
    public class FilePathRepository : Repository<FilePath>, IFilePathRepository
    {
        private readonly ApplicationDbContext _db;

        public FilePathRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(FilePath filePath)
        {
            var objFromDb = _db.FilePaths.FirstOrDefault(s => s.Id == filePath.Id);
            if (objFromDb != null)
            {
                objFromDb.Path = filePath.Path;
            }
        }
    }
}
