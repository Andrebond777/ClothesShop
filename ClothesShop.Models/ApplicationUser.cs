using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ClothesShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public bool IsMale { get; set; }
        [Required]
        public string Name { get; set; }

        [NotMapped]
        public string Role { get; set; }
    }
}
