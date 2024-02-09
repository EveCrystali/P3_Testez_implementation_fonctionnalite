﻿using Microsoft.AspNetCore.Mvc;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using P3AddNewFunctionalityDotNetCore.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using System;
using Microsoft.Extensions.Localization;
using Moq;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using System.Drawing;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using System.Globalization;
using static P3AddNewFunctionalityDotNetCore.Models.Services.ProductService;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Security.Policy;

namespace P3AddNewFunctionalityDotNetCore.Tests.UnitTests
{
    public class ProductServiceTests
    {
        private Mock<IProductService> _mockproductService;
        private Mock<ILanguageService> _mockLanguageService;
        private Mock<IStringLocalizer<OrderController>> _mockLocalizer;
        private ProductController _productController;

        public ProductServiceTests()
        {
            _mockproductService = new Mock<IProductService>();
            _mockLanguageService = new Mock<ILanguageService>();
            _mockLocalizer = new Mock<IStringLocalizer<OrderController>>();
            _productController = new ProductController(_mockproductService.Object, _mockLanguageService.Object);
        }

        [Fact]
        public void TestParseDoubleWithAutoDecimalSeparator_WhenInputHasAComma()
        {
            // Arrange
            string input = "123,45";
            // Act
            double result = ParseDoubleWithAutoDecimalSeparator(input);
            // Assert
            Assert.Equal(123.45, result);
        }

        [Fact]
        public void TestParseDoubleWithAutoDecimalSeparator_WhenInputHasADot()
        {
            // Arrange
            string input = "123.45";
            // Act
            double result = ParseDoubleWithAutoDecimalSeparator(input);
            // Assert
            Assert.Equal(123.45, result);
        }

        [Fact]
        public void TestParseDoubleWithAutoDecimalSeparator_WhenInputHasASeparator()
        {
            // Arrange
            string input = "12345";
            // Act
            double result = ParseDoubleWithAutoDecimalSeparator(input);
            // Assert
            Assert.Equal(12345, result);
        }

        [Fact]
        public void Create_AddOneProductWithDifferentTypesOfPrice_ProductAddedInList()
        {
            ProductViewModel productViewModel1 = new ProductViewModel()
            {   // Price has decimal dot
                Name = "NameTest",
                Price = "1.00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            ProductViewModel productViewModel2 = new ProductViewModel()
            {   // Price has decimal comma
                Name = "NameTest",
                Price = "1,00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 2
            };

            ProductViewModel productViewModel3 = new ProductViewModel()
            {   //Price has decimal dot none
                Name = "NameTest",
                Price = "1.",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 3
            };

            ProductViewModel productViewModel4 = new ProductViewModel()
            {   // Price has decimal comma none
                Name = "NameTest",
                Price = "1,",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 4
            };

            ProductViewModel productViewModel5 = new ProductViewModel()
            {   //Price is an integer
                Name = "NameTest",
                Price = "1",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 5
            };

            // Act
            var validationContext1 = new ValidationContext(productViewModel1, null, null);
            var validationResults1 = new List<ValidationResult>();
            bool isValid1 = Validator.TryValidateObject(productViewModel1, validationContext1, validationResults1, true);
            var result1 = _productController.Create(productViewModel1) as RedirectToActionResult;

            var validationContext2 = new ValidationContext(productViewModel2, null, null);
            var validationResults2 = new List<ValidationResult>();
            bool isValid2 = Validator.TryValidateObject(productViewModel2, validationContext2, validationResults2, true);
            var result2 = _productController.Create(productViewModel2) as RedirectToActionResult;

            var validationContext3 = new ValidationContext(productViewModel3, null, null);
            var validationResults3 = new List<ValidationResult>();
            bool isValid3 = Validator.TryValidateObject(productViewModel3, validationContext3, validationResults3, true);
            var result3 = _productController.Create(productViewModel3) as RedirectToActionResult;

            var validationContext4 = new ValidationContext(productViewModel4, null, null);
            var validationResults4 = new List<ValidationResult>();
            bool isValid4 = Validator.TryValidateObject(productViewModel4, validationContext4, validationResults4, true);
            var result4 = _productController.Create(productViewModel4) as RedirectToActionResult;

            var validationContext5 = new ValidationContext(productViewModel5, null, null);
            var validationResults5 = new List<ValidationResult>();
            bool isValid5 = Validator.TryValidateObject(productViewModel5, validationContext5, validationResults5, true);
            var result5 = _productController.Create(productViewModel5) as RedirectToActionResult;

            // Assert
            // For each product, we check if the model is considered as valid then if the product has been added to the list and if we are redirected to Admin

            Assert.True(isValid1, "Model should be valid because every field is well filled");
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(5));
            Assert.NotNull(result1);
            Assert.Equal("Admin", result1.ActionName);

            Assert.True(isValid2, "Model should be valid because every field is well filled");
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(5));
            Assert.NotNull(result2);
            Assert.Equal("Admin", result2.ActionName);

            Assert.True(isValid3, "Model should be valid because every field is well filled");
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(5));
            Assert.NotNull(result3);
            Assert.Equal("Admin", result3.ActionName);

            Assert.True(isValid4, "Model should be valid because every field is well filled");
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(5));
            Assert.NotNull(result4);
            Assert.Equal("Admin", result4.ActionName);

            Assert.True(isValid5, "Model should be valid because every field is well filled");
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(5));
            Assert.NotNull(result5);
            Assert.Equal("Admin", result5.ActionName);
        }

        private void ValidateModel(Controller controller, object model)
        {
            //The manual synchronization of validation errors with the ModelState is done to mimic the behavior of an MVC controller during an HTTP request.
            //When a form is submitted, the MVC framework automatically validates the model and populates the ModelState with any validation errors.
            //However, in a unit test environment, this doesn't happen automatically.

            var validationContext = new ValidationContext(model, null, null);
            var validationResults = new List<ValidationResult>();

            // Synchronize validation results with controller's ModelState
            if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
            {
                foreach (var validationResult in validationResults)
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage);
                    }
                }
            }
        }

        [Fact]
        public void Create_Add1ProductNameMissing_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with an empty name
            ProductViewModel productViewModelNoName = new ProductViewModel()
            {
                Name = "",
                Price = "1.00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1:  Validate product model
            ValidateModel(_productController, productViewModelNoName);

            // Assert 1: Ensure ModelState is invalid
            Assert.False(_productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = _productController.Create(productViewModelNoName);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Add1ProductPriceMissing_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with an empty price
            ProductViewModel productViewModelNoPrice = new ProductViewModel()
            {
                Name = "NameTest",
                Price = "",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1: Validate product model
            ValidateModel(_productController, productViewModelNoPrice);

            // Assert 1: Ensure ModelState is invalid
            Assert.False(_productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = _productController.Create(productViewModelNoPrice);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Add1ProductPriceNotDecimal_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with a non-decimal price
            ProductViewModel productViewModelNotDecimalPrice = new ProductViewModel()
            {
                Name = "NameTest",
                Price = "1.0001",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1 : Validate product model
            ValidateModel(_productController, productViewModelNotDecimalPrice);

            // Assert 1 : Ensure ModelState is invalid
            Assert.False(_productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = _productController.Create(productViewModelNotDecimalPrice);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Add1ProductPriceNotGreaterThanZero_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with a negative price
            ProductViewModel productViewModelNegativePrice = new ProductViewModel()
            {
                Name = "NameTest",
                Price = "-1.00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1 : Validate product model
            ValidateModel(_productController, productViewModelNegativePrice);

            // Assert 1 : Ensure ModelState is invalid
            Assert.False(_productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = _productController.Create(productViewModelNegativePrice);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Add1ProductMissingQuantity_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with an empty Stock
            ProductViewModel productViewModelMissingQuantity = new ProductViewModel()
            {
                Name = "NameTest",
                Price = "1.00",
                Stock = "",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1 : Validate product model
            ValidateModel(_productController, productViewModelMissingQuantity);

            // Assert 1 : Ensure ModelState is invalid
            Assert.False(_productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = _productController.Create(productViewModelMissingQuantity);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Add1ProductQuantityNotANumber_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with a non Integer product quantity
            ProductViewModel productViewModelQuantityNotANumber = new ProductViewModel()
            {
                Name = "NameTest",
                Price = "1.00",
                Stock = "1.5",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1 : Validate product model
            ValidateModel(_productController, productViewModelQuantityNotANumber);

            // Assert 1 : Ensure ModelState is invalid
            Assert.False(_productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = _productController.Create(productViewModelQuantityNotANumber);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Add1ProductQuantityNotGreaterThanZero_ModelStateInvalid()
        {
            ProductViewModel productViewModelQuantityNotGreaterThanZero = new ProductViewModel()
            {
                // Quantity is intentionally negative to test invalid ModelState
                Name = "NameTest",
                Price = "1.00",
                Stock = "-1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1 : Validate product model
            ValidateModel(_productController, productViewModelQuantityNotGreaterThanZero);

            // Assert 1 : Ensure ModelState is invalid
            Assert.False(_productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = _productController.Create(productViewModelQuantityNotGreaterThanZero);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        // TODO : Add test for DeleteProduct
    }
}