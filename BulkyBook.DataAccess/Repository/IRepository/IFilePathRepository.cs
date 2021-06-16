using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IFilePathRepository : IRepository<FilePath>
    {
        void Update(FilePath filePath);
    }
}
