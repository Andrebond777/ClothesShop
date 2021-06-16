using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BulkyBook.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string Consist { get; set; }
        public string CareConditions { get; set; }
        [Required]
        [Range(1, 10000000)]
        public double Price { get; set; }
        public List<FilePath> ImagesUrl { get; set; }
        public List<Color> Colors { get; set; }
        [Required]
        public int CategoryId { get; set; }      
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

    }
}
