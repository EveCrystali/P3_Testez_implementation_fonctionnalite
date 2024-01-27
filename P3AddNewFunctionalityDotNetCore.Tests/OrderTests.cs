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

namespace P2FixAnAppDotNetCode.Tests
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
        //Cart is empty but Form is valid
        public void Index_WhenCartIsEmpty_AddsModelError()
        {
            // Arrange
            Cart cart = new Cart();
            _mockLocalizer.Setup(l => l["CartEmpty"]).Returns(new LocalizedString("CartEmpty", "The cart is empty."));
            _controller = new OrderController(cart, _mockOrderService.Object, _mockLocalizer.Object);

            var order = new OrderViewModel()
            {
                Name = "NomDeTest",
                Address = "AdresseTest",
                City = "VilleTest",
                Zip = "ZipTest",
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
        //Cart is not empty and form is valid 
        public void Index_CartNotEmptyModelStateValid_ModelStateValid()
        {
            // Arrange
            Cart cart = new Cart();
            Product product = new Product();
            cart.AddItem(product, 1);
            _controller = new OrderController(cart, _mockOrderService.Object, _mockLocalizer.Object);
            var order = new OrderViewModel()
            {
                Name = "NomDeTest",
                Address = "AdresseTest",
                City = "VilleTest",
                Zip = "ZipTest",
                Country = "PaysTest",
            };

            // Act
            var result = _controller.Index(order) as ViewResult;

            // Assert
            Assert.True(_controller.ModelState.IsValid, "Not Empty Cart and Model is Valid");
        }


        /*
        //TODO
        [Fact]
        //Cart is not empty and form is not valid : name is missing ErrorMissingName
        public void Index_MissingName_ModelStateIsNotValid()
        {
            // Arrange
            Cart cart = new Cart();
            Product product = new Product();
            cart.AddItem(product, 1);
            _controller = new OrderController(cart, _mockOrderService.Object, _mockLocalizer.Object);
            var order = new OrderViewModel()
            {
                Name = "",
                Address = "AdresseTest",
                City = "VilleTest",
                Zip = "ZipTest",
                Country = "PaysTest",
            };

            // Act
            var result = _controller.Index(order) as ViewResult;
            var modelStateValidity = _controller.ModelState.GetValidationState(order.Name);
            modelStateValidity.

            // Assert
            Assert.False(modelStateValidity, "Not Empty Cart and Name is missing");

       
        }
        */
    }

}