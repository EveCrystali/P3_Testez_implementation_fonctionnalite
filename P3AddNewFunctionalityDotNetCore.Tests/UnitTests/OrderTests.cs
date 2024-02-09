using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using Xunit;
using Microsoft.Extensions.Localization;
using Moq;
using System.Linq;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using Castle.Components.DictionaryAdapter.Xml;

namespace P3AddNewFunctionalityDotNetCore.Tests.UnitTests
{
    public class OrderControllerTests
    {
        private Mock<ICart> _mockCart;
        private Mock<IOrderService> _mockOrderService;
        private Mock<IStringLocalizer<OrderController>> _mockLocalizer;
        private OrderController _controller;

        public OrderControllerTests()
        {
            _mockCart = new Mock<ICart>();
            _mockOrderService = new Mock<IOrderService>();
            _mockLocalizer = new Mock<IStringLocalizer<OrderController>>();
            _controller = new OrderController(_mockCart.Object, _mockOrderService.Object, _mockLocalizer.Object);
        }

        [Fact]
        public void Index_WhenCartIsEmpty_AddsModelError()
        {
            // Arrange
            Cart cart = new();
            _mockLocalizer.Setup(l => l["CartEmpty"]).Returns(new LocalizedString("CartEmpty", "The cart is empty."));
            _controller = new OrderController(cart, _mockOrderService.Object, _mockLocalizer.Object);

            var order = new OrderViewModel()
            {
                Name = "NomDeTest",
                Address = "AdresseTest",
                City = "VilleTest",
                Zip = "75000",
                Country = "PaysTest",
            };

            // Act
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal("The cart is empty.", _controller.ModelState[""].Errors.First().ErrorMessage);
            Assert.NotNull(result);
            Assert.Equal(order, result?.Model);
        }

        [Fact]
        public void Index_CartNotEmptyModelStateValid_ModelStateValid()
        {
            // Arrange
            Cart cart = new();
            Product product = new()
            {
                Name = "ProductNameTest",
                Price = 1.00,
                Quantity = 1,
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1,
            };
            cart.AddItem(product, 1);
            _controller = new OrderController(cart, _mockOrderService.Object, _mockLocalizer.Object);
            var order = new OrderViewModel()
            {
                Name = "NomDeTest",
                Address = "AdresseTest",
                City = "VilleTest",
                Zip = "75000",
                Country = "PaysTest",
            };

            // Act
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.True(_controller.ModelState.IsValid, "Not Empty Cart and Model is Valid");
        }

        [Fact]
        public void Index_MissingName_ModelStateIsNotValid()
        {
            // Arrange
            Cart cart = new();
            Product product = new()
            {
                Name = "ProductNameTest",
                Price = 1.00,
                Quantity = 1,
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1,
            };

            cart.AddItem(product, 1);
            _controller = new OrderController(cart, _mockOrderService.Object, _mockLocalizer.Object);
            var order = new OrderViewModel()
            {
                Name = "",
                Address = "AdresseTest",
                City = "VilleTest",
                Zip = "75000",
                Country = "CountryTest",
            };

            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Act
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.False(isValid, "Model should be invalid when Name is missing");
        }

        [Fact]
        public void Index_MissingAddress_ModelStateIsNotValid()
        {
            // Arrange
            Cart cart = new();
            Product product = new()
            {
                Name = "ProductNameTest",
                Price = 1.00,
                Quantity = 1,
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1,
            };

            cart.AddItem(product, 1);
            _controller = new OrderController(cart, _mockOrderService.Object, _mockLocalizer.Object);
            var order = new OrderViewModel()
            {
                Name = "NameTest",
                Address = "",
                City = "VilleTest",
                Zip = "75000",
                Country = "CountryTest",
            };

            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Act
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.False(isValid, "Model should be invalid when Address is missing");
        }

        [Fact]
        public void Index_MissingCity_ModelStateIsNotValid()
        {
            // Arrange
            Cart cart = new();
            Product product = new()
            {
                Name = "ProductNameTest",
                Price = 1.00,
                Quantity = 1,
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1,
            };

            cart.AddItem(product, 1);
            _controller = new OrderController(cart, _mockOrderService.Object, _mockLocalizer.Object);
            var order = new OrderViewModel()
            {
                Name = "NameTest",
                Address = "AdresseTest",
                City = "",
                Zip = "75000",
                Country = "CountryTest",
            };

            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Act
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.False(isValid, "Model should be invalid when City is missing");
        }

        [Fact]
        public void Index_MissingZip_ModelStateIsNotValid()
        {
            // Arrange
            Cart cart = new();
            Product product = new()
            {
                Name = "ProductNameTest",
                Price = 1.00,
                Quantity = 1,
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1,
            };

            cart.AddItem(product, 1);
            _controller = new OrderController(cart, _mockOrderService.Object, _mockLocalizer.Object);
            var order = new OrderViewModel()
            {
                Name = "NameTest",
                Address = "AdresseTest",
                City = "CityTest",
                Zip = "",
                Country = "CountryTest",
            };

            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Act
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.False(isValid, "Model should be invalid when Zip is missing");
        }

        [Fact]
        public void Index_UnvalidZip_ModelStateIsNotValid()
        {
            // Arrange
            Cart cart = new();
            Product product = new()
            {
                Name = "ProductNameTest",
                Price = 1.00,
                Quantity = 1,
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1,
            };

            cart.AddItem(product, 1);
            _controller = new OrderController(cart, _mockOrderService.Object, _mockLocalizer.Object);
            var order = new OrderViewModel()
            {
                Name = "NameTest",
                Address = "AdresseTest",
                City = "CityTest",
                Zip = "666",
                Country = "CountryTest",
            };

            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Act
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.False(isValid, "Model should be invalid when Zip is false");
        }

        [Fact]
        public void Index_MissingCountry_ModelStateIsNotValid()
        {
            // Arrange
            Cart cart = new();
            Product product = new()
            {
                Name = "ProductNameTest",
                Price = 1.00,
                Quantity = 1,
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1,
            };

            cart.AddItem(product, 1);
            _controller = new OrderController(cart, _mockOrderService.Object, _mockLocalizer.Object);
            var order = new OrderViewModel()
            {
                Name = "NameTest",
                Address = "AdresseTest",
                City = "CityTest",
                Zip = "ZipTest",
                Country = "",
            };

            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Act
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.False(isValid, "Model should be invalid when Country is missing");
        }
    }
}