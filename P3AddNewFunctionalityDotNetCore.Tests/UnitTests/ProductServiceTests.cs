using Microsoft.AspNetCore.Mvc;
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
            // Initialize the mocks
            _mockproductService = new Mock<IProductService>();
            _mockLanguageService = new Mock<ILanguageService>();
            _mockLocalizer = new Mock<IStringLocalizer<OrderController>>();

            // Initialize ProductController with mocked dependencies
            _productController = new ProductController(_mockproductService.Object, _mockLanguageService.Object);
        }

        [Fact]
        // Verify the different scenarios where a product is added and it should be added to the list
        public void Create_Add1Product_ProductAddedInList()
        {

            ProductViewModel product1 = new ProductViewModel()
            {   //Decimal dot
                Name = "NameTest",
                Price = "1.00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            ProductViewModel product2 = new ProductViewModel()
            {   //Decimal comma
                Name = "NameTest",
                Price = "1,00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 2
            };

            ProductViewModel product3 = new ProductViewModel()
            {   //Decimal dot none
                Name = "NameTest",
                Price = "1.",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 3
            };

            ProductViewModel product4 = new ProductViewModel()
            {   //Decimal comma none
                Name = "NameTest",
                Price = "1,",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 4
            };

            ProductViewModel product5 = new ProductViewModel()
            {   //Integer
                Name = "NameTest",
                Price = "1",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 5
            };


            // Act
            var validationContext1 = new ValidationContext(product1, null, null);
            var validationResults1 = new List<ValidationResult>();
            bool isValid1 = Validator.TryValidateObject(product1, validationContext1, validationResults1, true);
            // Create the product1 
            var result1 = _productController.Create(product1) as RedirectToActionResult;

            var validationContext2 = new ValidationContext(product2, null, null);
            var validationResults2 = new List<ValidationResult>();
            bool isValid2 = Validator.TryValidateObject(product2, validationContext2, validationResults2, true);
            // Create the product2
            var result2 = _productController.Create(product2) as RedirectToActionResult;

            var validationContext3 = new ValidationContext(product3, null, null);
            var validationResults3 = new List<ValidationResult>();
            bool isValid3 = Validator.TryValidateObject(product3, validationContext3, validationResults3, true);
            // Create the product3 
            var result3 = _productController.Create(product3) as RedirectToActionResult;

            var validationContext4 = new ValidationContext(product4, null, null);
            var validationResults4 = new List<ValidationResult>();
            bool isValid4 = Validator.TryValidateObject(product4, validationContext4, validationResults4, true);
            // Create the product4
            var result4 = _productController.Create(product4) as RedirectToActionResult;

            var validationContext5 = new ValidationContext(product5, null, null);
            var validationResults5 = new List<ValidationResult>();
            bool isValid5 = Validator.TryValidateObject(product5, validationContext5, validationResults5, true);
            // Create the product5
            var result5 = _productController.Create(product5) as RedirectToActionResult;


            // Assert

            // Let's verify that the model is considered as: valid
            Assert.True(isValid1, "Model should be valid because every field is well filled");
            // Let's verify that the product1 has been added to the list 
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(5));
            Assert.NotNull(result1);
            Assert.Equal("Admin", result1.ActionName);

            // Let's verify that the model is considered as: valid
            Assert.True(isValid2, "Model should be valid because every field is well filled");
            // Let's verify that the product2 has been added to the list 
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(5));
            Assert.NotNull(result2);
            Assert.Equal("Admin", result2.ActionName);

            Assert.True(isValid3, "Model should be valid because every field is well filled");
            // Let's verify that the product3 has been added to the list 
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(5));
            Assert.NotNull(result3);
            Assert.Equal("Admin", result3.ActionName);

            Assert.True(isValid4, "Model should be valid because every field is well filled");
            // Let's verify that the product4 has been added to the list 
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(5));
            Assert.NotNull(result4);
            Assert.Equal("Admin", result4.ActionName);

            Assert.True(isValid5, "Model should be valid because every field is well filled");
            // Let's verify that the product5 has been added to the list 
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(5));
            Assert.NotNull(result5);
            Assert.Equal("Admin", result5.ActionName);

        }

        [Fact]
        // Tests the scenario where in the creation of the product the name field is missing so the model state should be invalid and the product should not be created
        public void Create_Add1ProductNameMissing_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with an empty name
            ProductViewModel product = new ProductViewModel()
            {
                // Name is intentionally left empty to test invalid ModelState
                Name = "",
                Price = "1.00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1: UnValidating the ModelState
            // We use TryValidateObject due to model state updating behavior in unit tests (otherwise it is considered as valid or Unvalidated meaning not checked)
            var validationContext = new ValidationContext(product, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);
            // Create the product 
            var result = _productController.Create(product) as RedirectToActionResult;

            // Assert 1: ModelState Validation
            // Ensure that the model state is considered invalid because the 'Name' field is empty
            Assert.False(isValid, "Model should be Invalid because Name field is empty");


            // Arrange 2: Manipulating ModelState for Testing that the product is not created (saved actually)
            // Now we know that the ModelState is well validated (here he is unvalid MissingName)
            // Meaning we can controll it to forced if to be invalid so that we can check that Create(product) is not going to save the product 
            // We check that Save is no more called meaning Create(product) take so the good if statement).
            var validationContext2 = new ValidationContext(product, null, null);
            var validationResults2 = new List<ValidationResult>();
            bool isValid2 = Validator.TryValidateObject(product, validationContext2, validationResults2, true);

            // Manual synchronization of validation errors with the ModelState
            foreach (var validationResult2 in validationResults2)
            {
                foreach (var memberName in validationResult2.MemberNames)
                {
                    _productController.ModelState.AddModelError(memberName, validationResult2.ErrorMessage);
                }
            }

            // Act 2: Create the product
            var result2 = _productController.Create(product);

            // Assert 3:
            //Verification and Result Type
            // Verify that the 'SaveProduct' method is not called on the 'IProductService' because the model state is invalid
            // ensure that a 'ViewResult' is returned for an invalid model
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Once); //Once becasue SaveProduct has already been called in the previous act : Act 1. 
            Assert.IsType<ViewResult>(result2); //
        }

        [Fact]
        // Tests the scenario where in the creation of the product the price field is missing so the model state should be invalid and the product should not be created
        public void Create_Add1ProductPriceMissing_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with an empty price
            ProductViewModel product = new ProductViewModel()
            {
                // Price is intentionally left empty to test invalid ModelState
                Name = "NameTest",
                Price = "",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1: UnValidating the ModelState
            // We use TryValidateObject due to model state updating behavior in unit tests (otherwise it is considered as valid or Unvalidated meaning not checked)
            var validationContext = new ValidationContext(product, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);
            // Create the product 
            var result = _productController.Create(product) as RedirectToActionResult;

            // Assert 1: ModelState Validation
            // Ensure that the model state is considered invalid because the 'Price' field is empty
            Assert.False(isValid, "Model should be Invalid because Price field is empty");


            // Arrange 2: Manipulating ModelState for Testing that the product is not created (saved actually)
            // Now we know that the ModelState is well validated (here he is unvalid MissingPrice)
            // Meaning we can controll it to forced if to be invalid so that we can check that Create(product) is not going to save the product 
            // We check that Save is no more called meaning Create(product) take so the good if statement).
            var validationContext2 = new ValidationContext(product, null, null);
            var validationResults2 = new List<ValidationResult>();
            bool isValid2 = Validator.TryValidateObject(product, validationContext2, validationResults2, true);

            // Manual synchronization of validation errors with the ModelState
            foreach (var validationResult2 in validationResults2)
            {
                foreach (var memberName in validationResult2.MemberNames)
                {
                    _productController.ModelState.AddModelError(memberName, validationResult2.ErrorMessage);
                }
            }

            // Act 2: Create the product
            var result2 = _productController.Create(product);

            // Assert 3:
            //Verification and Result Type
            // Verify that the 'SaveProduct' method is not called on the 'IProductService' because the model state is invalid
            // ensure that a 'ViewResult' is returned for an invalid model
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Once); //Once becasue SaveProduct has already been called in the previous act : Act 1. 
            Assert.IsType<ViewResult>(result2); //

        }

        [Fact]
        // Tests the scenario where a product is added with a non-decimal product price and the model state should be invalid and the product should not be created
        public void Create_Add1ProductPriceNotDecimal_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with a non-decimal price
            ProductViewModel product = new ProductViewModel()
            {
                // Price is intentionally non-decimal to test invalid ModelState
                Name = "NameTest",
                Price = "1.0001",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1: UnValidating the ModelState
            // We use TryValidateObject due to model state updating behavior in unit tests (otherwise it is considered as valid or Unvalidated meaning not checked)
            var validationContext = new ValidationContext(product, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);
            // Create the product 
            var result = _productController.Create(product) as RedirectToActionResult;

            // Assert 1: ModelState Validation
            // Ensure that the model state is considered invalid because the 'Price' field is not decimal
            Assert.False(isValid, "Model should be Invalid because PriceNotANumber");


            // Arrange 2: Manipulating ModelState for Testing that the product is not created (saved actually)
            // Now we know that the ModelState is well validated (here he is unvalid PriceNotANumber)
            // Meaning we can controll it to forced if to be invalid so that we can check that Create(product) is not going to save the product 
            // We check that Save is no more called meaning Create(product) take so the good if statement).
            var validationContext2 = new ValidationContext(product, null, null);
            var validationResults2 = new List<ValidationResult>();
            bool isValid2 = Validator.TryValidateObject(product, validationContext2, validationResults2, true);

            // Manual synchronization of validation errors with the ModelState
            foreach (var validationResult2 in validationResults2)
            {
                foreach (var memberName in validationResult2.MemberNames)
                {
                    _productController.ModelState.AddModelError(memberName, validationResult2.ErrorMessage);
                }
            }

            // Act 2: Create the product
            var result2 = _productController.Create(product);

            // Assert 3:
            //Verification and Result Type
            // Verify that the 'SaveProduct' method is not called on the 'IProductService' because the model state is invalid
            // ensure that a 'ViewResult' is returned for an invalid model
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Once); //Once becasue SaveProduct has already been called in the previous act : Act 1. 
            Assert.IsType<ViewResult>(result2); //

        }

        [Fact]
        // Tests the scenario where a product is added with a product price not greater than zero and the model state should be invalid and the product should not be created
        public void Create_Add1ProductPriceotGreaterThanZero_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with a negative price
            ProductViewModel product = new ProductViewModel()
            {
                // Price is intentionally non-decimal to test invalid ModelState
                Name = "NameTest",
                Price = "-1.00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1: UnValidating the ModelState
            // We use TryValidateObject due to model state updating behavior in unit tests (otherwise it is considered as valid or Unvalidated meaning not checked)
            var validationContext = new ValidationContext(product, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);
            // Create the product 
            var result = _productController.Create(product) as RedirectToActionResult;

            // Assert 1: ModelState Validation
            // Ensure that the model state is considered invalid because the 'Price' field is negative
            Assert.False(isValid, "Model should be Invalid because PriceNotGreaterThanZero");


            // Arrange 2: Manipulating ModelState for Testing that the product is not created (saved actually)
            // Now we know that the ModelState is well validated (here he is unvalid PriceNotGreaterThanZero)
            // Meaning we can controll it to forced if to be invalid so that we can check that Create(product) is not going to save the product 
            // We check that Save is no more called meaning Create(product) take so the good if statement).
            var validationContext2 = new ValidationContext(product, null, null);
            var validationResults2 = new List<ValidationResult>();
            bool isValid2 = Validator.TryValidateObject(product, validationContext2, validationResults2, true);

            // Manual synchronization of validation errors with the ModelState
            foreach (var validationResult2 in validationResults2)
            {
                foreach (var memberName in validationResult2.MemberNames)
                {
                    _productController.ModelState.AddModelError(memberName, validationResult2.ErrorMessage);
                }
            }

            // Act 2: Create the product
            var result2 = _productController.Create(product);

            // Assert 3:
            //Verification and Result Type
            // Verify that the 'SaveProduct' method is not called on the 'IProductService' because the model state is invalid
            // ensure that a 'ViewResult' is returned for an invalid model
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Once); //Once becasue SaveProduct has already been called in the previous act : Act 1. 
            Assert.IsType<ViewResult>(result2); //

        }

        [Fact]
        // Tests the scenario where a product is added with a missing product quantity and the model state should be invalid and the product should not be created
        public void Create_Add1ProductQuantityMissing_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with an empty Stock
            ProductViewModel product = new ProductViewModel()
            {
                // Stock is intentionally left empty to test invalid ModelState
                Name = "NameTest",
                Price = "1.00",
                Stock = "",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1: UnValidating the ModelState
            // We use TryValidateObject due to model state updating behavior in unit tests (otherwise it is considered as valid or Unvalidated meaning not checked)
            var validationContext = new ValidationContext(product, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);
            // Create the product 
            var result = _productController.Create(product) as RedirectToActionResult;

            // Assert 1: ModelState Validation
            // Ensure that the model state is considered invalid because the 'Stock' field is empty
            Assert.False(isValid, "Model should be Invalid because Stock field is empty");


            // Arrange 2: Manipulating ModelState for Testing that the product is not created (saved actually)
            // Now we know that the ModelState is well validated (here he is unvalid MissingName)
            // Meaning we can controll it to forced if to be invalid so that we can check that Create(product) is not going to save the product 
            // We check that Save is no more called meaning Create(product) take so the good if statement).
            var validationContext2 = new ValidationContext(product, null, null);
            var validationResults2 = new List<ValidationResult>();
            bool isValid2 = Validator.TryValidateObject(product, validationContext2, validationResults2, true);

            // Manual synchronization of validation errors with the ModelState
            foreach (var validationResult2 in validationResults2)
            {
                foreach (var memberName in validationResult2.MemberNames)
                {
                    _productController.ModelState.AddModelError(memberName, validationResult2.ErrorMessage);
                }
            }

            // Act 2: Create the product
            var result2 = _productController.Create(product);

            // Assert 3:
            //Verification and Result Type
            // Verify that the 'SaveProduct' method is not called on the 'IProductService' because the model state is invalid
            // ensure that a 'ViewResult' is returned for an invalid model
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Once); //Once becasue SaveProduct has already been called in the previous act : Act 1. 
            Assert.IsType<ViewResult>(result2); //
        }

        [Fact]
        // Tests the scenario where a product is added with a non Integer product quantity and the model state should be invalid and the product should not be created
        public void Create_Add1ProductQuantityNotANumber_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with a non Integer product quantity
            ProductViewModel product = new ProductViewModel()
            {
                // Quantity is intentionally not an integer to test invalid ModelState
                Name = "NameTest",
                Price = "1.00",
                Stock = "1.5",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1: UnValidating the ModelState
            // We use TryValidateObject due to model state updating behavior in unit tests (otherwise it is considered as valid or Unvalidated meaning not checked)
            var validationContext = new ValidationContext(product, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);
            // Create the product 
            var result = _productController.Create(product) as RedirectToActionResult;

            // Assert 1: ModelState Validation
            // Ensure that the model state is considered invalid because the 'Quantity' field is not an integer
            Assert.False(isValid, "Model should be Invalid because QuantityNotAnInteger");


            // Arrange 2: Manipulating ModelState for Testing that the product is not created (saved actually)
            // Now we know that the ModelState is well validated (here he is unvalid QuantityNotAnInteger)
            // Meaning we can controll it to forced if to be invalid so that we can check that Create(product) is not going to save the product 
            // We check that Save is no more called meaning Create(product) take so the good if statement).
            var validationContext2 = new ValidationContext(product, null, null);
            var validationResults2 = new List<ValidationResult>();
            bool isValid2 = Validator.TryValidateObject(product, validationContext2, validationResults2, true);

            // Manual synchronization of validation errors with the ModelState
            foreach (var validationResult2 in validationResults2)
            {
                foreach (var memberName in validationResult2.MemberNames)
                {
                    _productController.ModelState.AddModelError(memberName, validationResult2.ErrorMessage);
                }
            }

            // Act 2: Create the product
            var result2 = _productController.Create(product);

            // Assert 3:
            //Verification and Result Type
            // Verify that the 'SaveProduct' method is not called on the 'IProductService' because the model state is invalid
            // ensure that a 'ViewResult' is returned for an invalid model
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Once); //Once becasue SaveProduct has already been called in the previous act : Act 1. 
            Assert.IsType<ViewResult>(result2); //

        }

        [Fact]
        // Tests the scenario where a product is added with a product quantity not greater than zero and the model state should be invalid and the product should not be created
        public void Create_Add1ProductQuantityNotGreaterThanZero_ModelStateInvalid()
        {
            //Arrange 1: initialize the product with a non negative product quantity
            ProductViewModel product = new ProductViewModel()
            {
                // Quantity is intentionally negative to test invalid ModelState
                Name = "NameTest",
                Price = "1.00",
                Stock = "-1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1: UnValidating the ModelState
            // We use TryValidateObject due to model state updating behavior in unit tests (otherwise it is considered as valid or Unvalidated meaning not checked)
            var validationContext = new ValidationContext(product, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);
            // Create the product 
            var result = _productController.Create(product) as RedirectToActionResult;

            // Assert 1: ModelState Validation
            // Ensure that the model state is considered invalid because the 'Quantity' field is negative
            Assert.False(isValid, "Model should be Invalid because QuantityNotAnInteger");


            // Arrange 2: Manipulating ModelState for Testing that the product is not created (saved actually)
            // Now we know that the ModelState is well validated (here he is unvalid QuantityNotGreaterThanZero)
            // Meaning we can controll it to forced if to be invalid so that we can check that Create(product) is not going to save the product 
            // We check that Save is no more called meaning Create(product) take so the good if statement).
            var validationContext2 = new ValidationContext(product, null, null);
            var validationResults2 = new List<ValidationResult>();
            bool isValid2 = Validator.TryValidateObject(product, validationContext2, validationResults2, true);

            // Manual synchronization of validation errors with the ModelState
            foreach (var validationResult2 in validationResults2)
            {
                foreach (var memberName in validationResult2.MemberNames)
                {
                    _productController.ModelState.AddModelError(memberName, validationResult2.ErrorMessage);
                }
            }

            // Act 2: Create the product
            var result2 = _productController.Create(product);

            // Assert 3:
            //Verification and Result Type
            // Verify that the 'SaveProduct' method is not called on the 'IProductService' because the model state is invalid
            // ensure that a 'ViewResult' is returned for an invalid model
            _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Once); //Once becasue SaveProduct has already been called in the previous act : Act 1. 
            Assert.IsType<ViewResult>(result2); //

        }

        [Fact]
        public void TestParseDoubleWithAutoDecimalSeparator_WithComma()
        {
            // Arrange
            string input = "123,45";
            // Act
            double result = ParseDoubleWithAutoDecimalSeparator(input);
            // Assert
            Assert.Equal(123.45, result);
        }

        [Fact]
        public void TestParseDoubleWithAutoDecimalSeparator_WithDot()
        {
            // Arrange
            string input = "123.45";
            // Act
            double result = ParseDoubleWithAutoDecimalSeparator(input);
            // Assert
            Assert.Equal(123.45, result);
        }

        [Fact]
        public void TestParseDoubleWithAutoDecimalSeparator_WithoutSeparator()
        {
            // Arrange
            string input = "12345";
            // Act
            double result = ParseDoubleWithAutoDecimalSeparator(input);
            // Assert
            Assert.Equal(12345, result);
        }
    }
}