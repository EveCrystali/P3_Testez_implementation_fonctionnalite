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

        //[Required(ErrorMessage = "ErrorMissingName")]
        public string Name { get; set; }

        //[Required(ErrorMessage = "ErrorMissingPrice")]
        //[RegularExpression(@"^\d+.\d{0,2}$", ErrorMessage = "ErrorPriceValue")]
        //[Range(0.01, double.MaxValue, ErrorMessage = "ErrorPriceValue")]

        public double Price { get; set; }


        //[Required(ErrorMessage = "ErrorMissingStock")]
        //[RegularExpression(@"^\d+$", ErrorMessage = "ErrorStockValue")]
        //[Range(1, int.MaxValue, ErrorMessage = "ErrorStockValue")]
        public int Quantity { get; set; }

        public virtual ICollection<OrderLine> OrderLine { get; set; }


    }


  

    
}
