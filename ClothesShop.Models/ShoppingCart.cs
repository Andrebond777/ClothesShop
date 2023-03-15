using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ClothesShop.Models
{
    public class ShoppingCart
    {
        public ShoppingCart()
        {
            Count = 1;
            //Product = new Product();
            //Product.Price = 0;
        }
        [Key]
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public Double OneTypeTotal/* => Product.Price * Count;*/{ get; set; }

        [Range(1,1000,ErrorMessage ="Please enter a value between 1 and 1000")]
        public int Count { get; set; }

        [NotMapped]
        public double Price { get; set; }

    }
}
