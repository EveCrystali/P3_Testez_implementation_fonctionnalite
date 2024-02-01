using P3AddNewFunctionalityDotNetCore.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace P3AddNewFunctionalityDotNetCore.Models
{
    public class Cart : ICart
    {
        private readonly List<CartLine> _cartLines;

        public Cart()
        {
            _cartLines = new List<CartLine>();
        }

        public int GetProductQuantityInCart(Product product)
        {
            CartLine line = _cartLines.FirstOrDefault(p => p.Product.Id == product.Id);
            if (line == null) { return 0; }
            else { return line.Quantity; }
        }

        public void AddItem(Product product, int quantity)
        {
            CartLine line = _cartLines.FirstOrDefault(p => p.Product.Id == product.Id);

            if (line == null)
            {
                if (product.Quantity < quantity)
                {
                    return;
                }
                _cartLines.Add(new CartLine { Product = product, Quantity = quantity });
            }
            else
            {
                // Check if the total quantity in the cart exceeds the available stock
                if (product.Quantity < line.Quantity + quantity)
                {
                    return;
                }
                line.Quantity += quantity;
            }
        }

        public void RemoveLine(Product product) => _cartLines.RemoveAll(l => l.Product.Id == product.Id);

        public double GetTotalValue()
        {
            return _cartLines.Any() ? _cartLines.Sum(l => l.Product.Price) : 0;
        }

        public double GetAverageValue()
        {
            return _cartLines.Any() ? _cartLines.Average(l => l.Product.Price) : 0;
        }

        public void Clear() => _cartLines.Clear();

        public IEnumerable<CartLine> Lines => _cartLines;
    }

    public class CartLine
    {
        public int OrderLineId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}