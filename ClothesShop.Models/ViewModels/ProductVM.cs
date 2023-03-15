﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClothesShop.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }
        public IEnumerable<SelectListItem> MainCategoryList { get; set; }
        public IEnumerable<SelectListItem> SubCategoryList { get; set; }
    }
}
