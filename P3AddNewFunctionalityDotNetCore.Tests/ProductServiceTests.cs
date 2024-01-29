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


namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class ProductServiceTests
    {

        private Mock<IProductService> _mockproductService;
        private Mock <ILanguageService> _mockLanguageService;
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
        // TODO Tests the scenario where a product is added and it should be added to the list
        public void Create_Add1Product_ProductAddedInList()
        {

            ProductViewModel product = new ProductViewModel()
            {
                Name = "NameTest",
                Price = "1.00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };


            // Act
            var validationContext = new ValidationContext(product, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);
                // Create the product 
                var result = _productController.Create(product) as RedirectToActionResult;


            // Assert
            // Let's verify that the model is considered as: valid
            Assert.True(isValid, "Model should be valid because every field is well filled");
                // Let's verify that the product has been added to the list 
                _mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Once);
                Assert.NotNull(result);
                Assert.Equal("Admin", result.ActionName);

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


        // TODO Tests the scenario where a product is added whereas it already exists in the list : only the quantity should be updated

    }
}