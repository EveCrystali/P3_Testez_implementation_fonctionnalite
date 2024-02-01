using System.Linq;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    /// <summary>
    /// The Cart test class
    /// </summary>
    public class CartTests
    {
        [Fact]
        public void AddItemInCart_Add2ItemID1_LinesCount1Quantity5()
        {
            //Arrange
            Cart cart = new();
            Product product1 = new()
            {
                Id = 1,
                Price = 1,
                Quantity = 10,
                Name = "name1",
                Description = "description1",
                Details = "details1"
            };

            //Act
            cart.AddItem(product1, 1);
            cart.AddItem(product1, 1);
            cart.AddItem(product1, 3);

            //Assert
            Assert.NotEmpty(cart.Lines);
            Assert.Single(cart.Lines);
            Assert.Equal(5, cart.Lines.First(p => p.Product.Name == "name1").Quantity);
        }

        [Fact]
        public void AddItemInCart_Add2DifferentItems_LinesCount2Quantity1()
        {
            //Arrange
            Cart cart = new();
            Product product1 = new()
            {
                Id = 1,
                Price = 1,
                Quantity = 1,
                Name = "name1",
                Description = "description1",
                Details = "details1"
            };
            Product product2 = new()
            {
                Id = 2,
                Price = 2,
                Quantity = 2,
                Name = "name2",
                Description = "description2",
                Details = "details2"
            };

            //Act
            cart.AddItem(product1, 1);
            cart.AddItem(product2, 2);

            //Assert
            Assert.Equal(1, cart.Lines.First().Quantity);
            Assert.Equal(2, cart.Lines.First(p => p.Product.Id == 2).Quantity);
            Assert.Equal(2, cart.Lines.Count());
        }

        [Fact]
        public void AddItemInCart_NoStock_NotAdd()
        {
            //Arrange
            Cart cart = new();
            Product product1 = new()
            {
                Id = 1,
                Price = 1,
                Quantity = 1,
                Name = "name1",
                Description = "description1",
                Details = "details1"
            };
            Product product2 = new()
            {
                Id = 2,
                Price = 2,
                Quantity = 0, // Not in stock
                Name = "name2",
                Description = "description2",
                Details = "details2"
            };

            //Act
            cart.AddItem(product1, 1);
            cart.AddItem(product2, 1);

            //Assert
            Assert.Equal(1, cart.Lines.First().Quantity);
            Assert.Single(cart.Lines); // Check that no lines have been added since the product is no longer in stock (so there is only item 1)
            Assert.DoesNotContain(cart.Lines, p => p.Product.Id == 2);
        }

        [Fact]
        public void AddItemInCart_NoEnoughStock_MaximumAdd()
        {
            //Arrange
            Cart cart = new();
            Product product1 = new()
            {
                Id = 1,
                Price = 1,
                Quantity = 1,
                Name = "name1",
                Description = "description1",
                Details = "details1"
            };
            Product product2 = new()
            {
                Id = 2,
                Price = 2,
                Quantity = 2, // Not in stock
                Name = "name2",
                Description = "description2",
                Details = "details2"
            };

            //Act
            cart.AddItem(product1, 1);
            cart.AddItem(product2, 99); // We add 99 times the product2 which now has only 2 products in stock

            //Assert
            Assert.Equal(1, cart.Lines.First().Quantity);
            Assert.Equal(2, cart.Lines.First(p => p.Product.Id == 2).Quantity); // Check that the quantity of product 2 is 2

            //Act - again
            cart.AddItem(product2, 99); // We add product2 once again, which no longer has any stock due to the previous operation

            //Assert - again
            Assert.Equal(2, cart.Lines.First(p => p.Product.Name == "name2").Quantity); // Check that the quantity added for product 2 is indeed 2 - nothing has been added (and checking with name instead of id)
            // TODO checking that a error message is well displayed
        }
    }
}