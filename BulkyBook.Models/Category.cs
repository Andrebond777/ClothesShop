using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BulkyBook.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Category MainName")]
        [Required]
        [MaxLength(50)]
        public string MainName { get; set; }

        [Display(Name = "Category SubName")]
        [Required]
        [MaxLength(50)]
        public string SubName { get; set; }
    }
}
