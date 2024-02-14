using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using Shouldly;
using Xunit;
using static P3AddNewFunctionalityDotNetCore.Models.Services.ProductService;

namespace P3AddNewFunctionalityDotNetCore.Tests.UnitTests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> mockProductRepository;
        private readonly Mock<IProductService> mockProductService;
        private readonly Mock<ILanguageService> mockLanguageService;
        private readonly Mock<IStringLocalizer<OrderController>> mockLocalizer;
        private readonly ProductController productController;
        private readonly List<Product> mockProductList = new();

        public ProductServiceTests()
        {
            mockProductRepository = new Mock<IProductRepository>();

            // Setup the behavior for SaveProduct method in ProductRepository
            mockProductRepository.Setup(repo => repo.SaveProduct(It.IsAny<Product>()))
                                 .Callback<Product>(product => mockProductList.Add(product));

            mockProductService = new Mock<IProductService>();

            // Setup the behavior for SaveProduct method in ProductService
            // When the SaveProduct method is called with any ProductViewModel, it will execute a callback that calls the SaveProduct method on a mocked ProductRepository, passing in a new Product.
            mockProductService.Setup(service => service.SaveProduct(It.IsAny<ProductViewModel>()))
                               .Callback<ProductViewModel>(_ => mockProductRepository.Object.SaveProduct(new Product()));

            mockProductService.Setup(service => service.DeleteProduct(It.IsAny<int>()))
                               .Callback<int>(productId =>
                               {
                                   mockProductRepository.Object.DeleteProduct(productId);
                                   var productToRemove = mockProductList.Find(p => p.Id == productId);
                                   if (productToRemove != null)
                                   {
                                       mockProductList.Remove(productToRemove);
                                   }
                               });
            mockProductRepository.Setup(repo => repo.GetAllProducts())
                         .Returns(mockProductList);

            mockLanguageService = new Mock<ILanguageService>();
            mockLocalizer = new Mock<IStringLocalizer<OrderController>>();
            productController = new ProductController(mockProductService.Object, mockLanguageService.Object);
        }

        [Fact]
        public void TestParseDoubleWithAutoDecimalSeparator_WhenInputHasAComma()
        {
            // Arrange
            const string input = "123,45";
            // Act
            double result = ParseDoubleWithAutoDecimalSeparator(input);
            // Assert
            Assert.Equal(123.45, result);
        }

        [Fact]
        public void TestParseDoubleWithAutoDecimalSeparator_WhenInputHasADot()
        {
            // Arrange
            const string input = "123.45";
            // Act
            double result = ParseDoubleWithAutoDecimalSeparator(input);
            // Assert
            Assert.Equal(123.45, result);
        }

        [Fact]
        public void TestParseDoubleWithAutoDecimalSeparator_WhenInputHasASeparator()
        {
            // Arrange
            const string input = "12345";
            // Act
            double result = ParseDoubleWithAutoDecimalSeparator(input);
            // Assert
            Assert.Equal(12345, result);
        }

        private static bool ValidateProduct(ProductViewModel productViewModel)
        {
            var validationContext = new ValidationContext(productViewModel, null, null);
            var validationResults = new List<ValidationResult>();
            return Validator.TryValidateObject(productViewModel, validationContext, validationResults, true);
        }

        [Fact]
        public void Create_AddOneProductWithDifferentTypesOfPrice_ProductAddedInList()
        {
            ProductViewModel productViewModel1 = new()
            {   // Price has decimal dot
                Name = "NameTest",
                Price = "1.00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            ProductViewModel productViewModel2 = new()
            {   // Price has decimal comma
                Name = "NameTest",
                Price = "1,00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 2
            };

            ProductViewModel productViewModel3 = new()
            {   //Price has decimal dot none
                Name = "NameTest",
                Price = "1.",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 3
            };

            ProductViewModel productViewModel4 = new()
            {   // Price has decimal comma none
                Name = "NameTest",
                Price = "1,",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 4
            };

            ProductViewModel productViewModel5 = new()
            {   //Price is an integer
                Name = "NameTest",
                Price = "1",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 5
            };

            // Act
            ValidateProduct(productViewModel1);
            var result1 = productController.Create(productViewModel1) as RedirectToActionResult;
            var result2 = productController.Create(productViewModel2) as RedirectToActionResult;
            var result3 = productController.Create(productViewModel3) as RedirectToActionResult;
            var result4 = productController.Create(productViewModel4) as RedirectToActionResult;
            var result5 = productController.Create(productViewModel5) as RedirectToActionResult;

            // Assert
            // Verify if the model is valid for each product and if the user is redirected to Admin.
            // Also ensure that all 5 products are passed as arguments to the SaveProduct method in ProductService and ProductRepository.
            Assert.True(ValidateProduct(productViewModel1), "Model should be valid because every field is well filled");
            Assert.NotNull(result1);
            Assert.Equal("Admin", result1.ActionName);

            Assert.True(ValidateProduct(productViewModel2), "Model should be valid because every field is well filled");
            Assert.NotNull(result2);
            Assert.Equal("Admin", result2.ActionName);

            Assert.True(ValidateProduct(productViewModel3), "Model should be valid because every field is well filled");
            Assert.NotNull(result3);
            Assert.Equal("Admin", result3.ActionName);

            Assert.True(ValidateProduct(productViewModel4), "Model should be valid because every field is well filled");
            Assert.NotNull(result4);
            Assert.Equal("Admin", result4.ActionName);

            Assert.True(ValidateProduct(productViewModel5), "Model should be valid because every field is well filled");
            Assert.NotNull(result5);
            Assert.Equal("Admin", result5.ActionName);

            mockProductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(5));
            mockProductRepository.Verify(repo => repo.SaveProduct(It.IsAny<Product>()), Times.Exactly(5));
            mockProductRepository.Object.GetAllProducts().Count().ShouldBe(5);
        }

        private static void ValidateModel(Controller controller, object model)
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
            ProductViewModel productViewModelNoName = new()
            {
                Name = "",
                Price = "1.00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1:  Validate product model
            ValidateModel(productController, productViewModelNoName);

            // Assert 1: Ensure ModelState is invalid
            Assert.False(productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = productController.Create(productViewModelNoName);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            mockProductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Add1ProductPriceMissing_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with an empty price
            ProductViewModel productViewModelNoPrice = new()
            {
                Name = "NameTest",
                Price = "",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1: Validate product model
            ValidateModel(productController, productViewModelNoPrice);

            // Assert 1: Ensure ModelState is invalid
            Assert.False(productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = productController.Create(productViewModelNoPrice);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            mockProductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Add1ProductPriceNotDecimal_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with a non-decimal price
            ProductViewModel productViewModelNotDecimalPrice = new()
            {
                Name = "NameTest",
                Price = "1.0001",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1 : Validate product model
            ValidateModel(productController, productViewModelNotDecimalPrice);

            // Assert 1 : Ensure ModelState is invalid
            Assert.False(productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = productController.Create(productViewModelNotDecimalPrice);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            mockProductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Add1ProductPriceNotGreaterThanZero_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with a negative price
            ProductViewModel productViewModelNegativePrice = new()
            {
                Name = "NameTest",
                Price = "-1.00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1 : Validate product model
            ValidateModel(productController, productViewModelNegativePrice);

            // Assert 1 : Ensure ModelState is invalid
            Assert.False(productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = productController.Create(productViewModelNegativePrice);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            mockProductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Add1ProductMissingQuantity_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with an empty Stock
            ProductViewModel productViewModelMissingQuantity = new()
            {
                Name = "NameTest",
                Price = "1.00",
                Stock = "",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1 : Validate product model
            ValidateModel(productController, productViewModelMissingQuantity);

            // Assert 1 : Ensure ModelState is invalid
            Assert.False(productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = productController.Create(productViewModelMissingQuantity);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            mockProductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Add1ProductQuantityNotANumber_ModelStateInvalidProductNotCreated()
        {
            //Arrange 1: initialize the product with a non Integer product quantity
            ProductViewModel productViewModelQuantityNotANumber = new()
            {
                Name = "NameTest",
                Price = "1.00",
                Stock = "1.5",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act 1 : Validate product model
            ValidateModel(productController, productViewModelQuantityNotANumber);

            // Assert 1 : Ensure ModelState is invalid
            Assert.False(productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = productController.Create(productViewModelQuantityNotANumber);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            mockProductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Add1ProductQuantityNotGreaterThanZero_ModelStateInvalid()
        {
            ProductViewModel productViewModelQuantityNotGreaterThanZero = new()
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
            ValidateModel(productController, productViewModelQuantityNotGreaterThanZero);

            // Assert 1 : Ensure ModelState is invalid
            Assert.False(productController.ModelState.IsValid, "Model should be invalid due to empty Name");

            // Act 2: Attempt to create product with invalid model
            var result = productController.Create(productViewModelQuantityNotGreaterThanZero);

            // Assert 2: Verify product is not saved and correct ViewResult is returned
            mockProductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_WhenClickingOnDelete_DeleteProduct()
        {
            ProductViewModel productViewModel1 = new() // This product is well defined
            {
                Name = "DeleteThis",
                Price = "1.00",
                Stock = "666",
                Description = "We create this product. We Assert. We delete. We Assert.",
                Details = "DetailsTest",
            };

            // Act
            productController.Create(productViewModel1);
            productController.DeleteProduct(productViewModel1.Id);

            // Assert
            mockProductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(1));
            mockProductRepository.Verify(service => service.SaveProduct(It.IsAny<Product>()), Times.Exactly(1));
            mockProductService.Verify(service => service.DeleteProduct(It.IsAny<int>()), Times.Exactly(1));
            mockProductRepository.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Exactly(1));
            mockProductRepository.Object.GetAllProducts().Count().ShouldBe(0);
        }
    }
}