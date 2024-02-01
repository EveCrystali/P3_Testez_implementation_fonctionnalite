using System;
using System.Collections.Generic;
using System.Linq;
using P3AddNewFunctionalityDotNetCore.Models.Entities;

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
            CartLine line = _cartLines.Find(p => p.Product.Id == product.Id);
            if (line == null) { return 0; }
            else { return line.Quantity; }
        }

        public void AddItem(Product product, int quantity)
        {
            CartLine line = _cartLines.Find(p => p.Product.Id == product.Id);

            if (line == null)
            {
                if (product.Quantity <= 0)
                {
                    return;
                }
                else
                {
                    int addQuantity = Math.Min(product.Quantity, quantity);
                    _cartLines.Add(new CartLine { Product = product, Quantity = addQuantity });
                }
            }
            else // Product already in_cart
            {
                // Check if the total quantity in the cart exceeds the available stock
                if (product.Quantity - line.Quantity > 0)
                {
                    // If so, adjust the quantity to the available stock
                    int addQuantity = Math.Min(product.Quantity - line.Quantity, quantity);
                    line.Quantity += addQuantity;
                }
                else
                {
                    return;
                }
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