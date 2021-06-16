using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IColorRepository : IRepository<Color>
    {
        void Update(Color color);
    }
}
