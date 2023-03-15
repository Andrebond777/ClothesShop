using System;
using System.Collections.Generic;
using System.Text;

namespace ClothesShop.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public List<ShoppingCart> ListCart { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
