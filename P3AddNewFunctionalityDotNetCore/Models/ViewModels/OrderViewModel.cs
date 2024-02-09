using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace P3AddNewFunctionalityDotNetCore.Models.ViewModels
{
    public class OrderViewModel
    {
        [BindNever]
        public int OrderId { get; set; }

        [BindNever]
        public ICollection<CartLine> Lines { get; set; }

        [Required(ErrorMessage = "ErrorMissingName")]
        public string Name { get; set; }

        [Required(ErrorMessage = "ErrorMissingAddress")]
        public string Address { get; set; }

        [Required(ErrorMessage = "ErrorMissingCity")]
        public string City { get; set; }

        // TODO Check type of zip
        [Required(ErrorMessage = "ErrorMissingZipCode")]
        [RegularExpression(@"^\d{5}(?:([-\s]|)\d{4})?$", ErrorMessage = "ErrorUnvalidZipCode")]
        public string Zip { get; set; }

        [Required(ErrorMessage = "ErrorMissingCountry")]
        public string Country { get; set; }

        [BindNever]
        public DateTime Date { get; set; }
    }
}
