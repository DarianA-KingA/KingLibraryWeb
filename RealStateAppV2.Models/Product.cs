using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingLibraryWeb.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        public string? Description { get; set; }
        [Required]
        public string? ISBM { get; set; }
        [Required]
        public string? Author { get; set; }
        [Required]
        [Range(1,10000)]
        [Display(Name = "List price")]

        public double ListPrice { get; set; }
        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 1-50")]

        public double Price { get; set; }
        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 51-100")]

        public double Price50 { get; set; }
        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 100+")]

        public double Price100 { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; }
        [Required]
        [Display(Name ="Category")]
        public int CategoryId { get; set; }
        [Required]
        [Display(Name = "Cover Type")]
        public int CoverTypeId { get; set; }

        //navigation propeties
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }
        [ForeignKey("CoverTypeId")]
        [ValidateNever]
        public CoverType CovertType { get; set; }



    }
}
