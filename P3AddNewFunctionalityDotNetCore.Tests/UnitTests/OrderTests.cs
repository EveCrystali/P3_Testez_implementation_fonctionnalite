using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests.UnitTests
{
    public class OrderControllerTests
    {
        private readonly Mock<ICart> _mockCart;
        private readonly Mock<IStringLocalizer<OrderController>> _mockLocalizer;
        private readonly Mock<IOrderService> _mockOrderService;
        private OrderController _controller;

        public OrderControllerTests()
        {
            _mockCart = new Mock<ICart>();
            _mockOrderService = new Mock<IOrderService>();
            _mockLocalizer = new Mock<IStringLocalizer<OrderController>>();
            _controller = new OrderController(_mockCart.Object, _mockOrderService.Object, _mockLocalizer.Object);
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
            OrderViewModel order = new()
            {
                Name = "NomTest",
                Address = "AdresseTest",
                City = "VilleTest",
                Zip = "75000",
                Country = "PaysTest",
            };
            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(order, validationContext, validationResults);

            // Act

            if (!isValid) { _controller.ModelState.AddModelError("Error", "Error added"); }
            var result = _controller.Index(order);

            // Assert
            Assert.True(isValid, "Model should be valid");
            Assert.IsType<RedirectToActionResult>(result);
            _mockOrderService.Verify(service => service.SaveOrder(It.IsAny<OrderViewModel>()), Times.Exactly(1));
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
            OrderViewModel order = new()
            {
                Name = "",
                Address = "AdresseTest",
                City = "VilleTest",
                Zip = "75000",
                Country = "CountryTest",
            };
            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(order, validationContext, validationResults);

            // Act
            if (!isValid) { _controller.ModelState.AddModelError("Error", "Error added"); }
            var result = _controller.Index(order);

            // Assert
            Assert.False(isValid, "Model should be invalid when Name is missing");
            Assert.IsType<ViewResult>(result);
            _mockOrderService.Verify(service => service.SaveOrder(It.IsAny<OrderViewModel>()), Times.Exactly(0));
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
            OrderViewModel order = new()
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
            if (!isValid) { _controller.ModelState.AddModelError("Error", "Error added"); }
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.False(isValid, "Model should be invalid when Address is missing");
            Assert.IsType<ViewResult>(result);
            _mockOrderService.Verify(service => service.SaveOrder(It.IsAny<OrderViewModel>()), Times.Exactly(0));
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
            OrderViewModel order = new()
            {
                Name = "NameTest",
                Address = "AdresseTest",
                City = "",
                Zip = "75000",
                Country = "CountryTest",
            };

            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(order, validationContext, validationResults);

            // Act
            if (!isValid) { _controller.ModelState.AddModelError("Error", "Error added"); }
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.False(isValid, "Model should be invalid when City is missing");
            Assert.IsType<ViewResult>(result);
            _mockOrderService.Verify(service => service.SaveOrder(It.IsAny<OrderViewModel>()), Times.Exactly(0));
        }

        [Fact]
        public void Index_InvalidZip_ModelStateIsNotValid()
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
            if (!isValid) { _controller.ModelState.AddModelError("Error", "Error added"); }
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.False(isValid, "Model should be invalid when Zip is false");
            Assert.IsType<ViewResult>(result);
            _mockOrderService.Verify(service => service.SaveOrder(It.IsAny<OrderViewModel>()), Times.Exactly(0));
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
            OrderViewModel order = new()
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
            if (!isValid) { _controller.ModelState.AddModelError("Error", "Error added"); }
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.False(isValid, "Model should be invalid when Country is missing");
            Assert.IsType<ViewResult>(result);
            _mockOrderService.Verify(service => service.SaveOrder(It.IsAny<OrderViewModel>()), Times.Exactly(0));
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
            OrderViewModel order = new()
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
            if (!isValid) { _controller.ModelState.AddModelError("Error", "Error added"); }
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.False(isValid, "Model should be invalid when Zip is missing");
            Assert.IsType<ViewResult>(result);
            _mockOrderService.Verify(service => service.SaveOrder(It.IsAny<OrderViewModel>()), Times.Exactly(0));
        }

        [Fact]
        public void Index_WhenCartIsEmpty_AddsModelError()
        {
            // Arrange
            Cart cart = new();
            _mockLocalizer.Setup(l => l["CartEmpty"]).Returns(new LocalizedString("CartEmpty", "The cart is empty."));
            _controller = new OrderController(cart, _mockOrderService.Object, _mockLocalizer.Object);

            OrderViewModel order = new()
            {
                Name = "NomDeTest",
                Address = "AdresseTest",
                City = "VilleTest",
                Zip = "75000",
                Country = "PaysTest",
            };
            var validationContext = new ValidationContext(order, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(order, validationContext, validationResults);

            // Act
            var result = _controller.Index(order);
            if (!isValid) { _controller.ModelState.AddModelError("Error", "Error added"); }

            // Assert
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal("The cart is empty.", _controller.ModelState[""].Errors.First().ErrorMessage);
            Assert.IsType<ViewResult>(result);
            _mockOrderService.Verify(service => service.SaveOrder(It.IsAny<OrderViewModel>()), Times.Exactly(0));
        }
    }
}