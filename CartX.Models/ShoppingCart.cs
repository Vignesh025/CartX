using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CartX.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public int productId { get; set; }
        [ForeignKey("productId")]
        [ValidateNever]
        public Product Product { get; set; }
        [Range(1,1000, ErrorMessage = "Please enter a valid quantity between 1 and 1000")]
        public int Count { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
