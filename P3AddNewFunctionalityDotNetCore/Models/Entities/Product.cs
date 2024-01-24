using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace P3AddNewFunctionalityDotNetCore.Models.Entities
{
    public partial class Product
    {
        public Product()
        {
            OrderLine = new HashSet<OrderLine>();
        }

        public int Id { get; set; }

        public string Description { get; set; }

        public string Details { get; set; }

        [Required(ErrorMessage = "MissingName")]
        public string Name { get; set; }

        [Required(ErrorMessage = "MissingPrice")]
        [RegularExpression(@"^\d+.\d{0,2}$", ErrorMessage = "InvalidPriceFormat")]
        [Range(0.01, double.MaxValue, ErrorMessage = "PriceNotGreaterThanZero")]

        public double Price { get; set; }


        [Required(ErrorMessage = "MissingQuantity")]
        [RegularExpression(@"^\d+$", ErrorMessage = "InvalidQuantityFormat")]
        [Range(1, int.MaxValue, ErrorMessage = "StockNotGreaterThanZero")]
        public int Quantity { get; set; }

        public virtual ICollection<OrderLine> OrderLine { get; set; }


    }


  

    
}
