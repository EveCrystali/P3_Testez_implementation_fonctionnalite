﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Test
{
    [CollectionDefinition("Tests")]
    public class TestCollection : ICollectionFixture<Integration>
    {
        // This class has no code and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    [Collection("Tests")]
    public class Integration : IDisposable
    {
        // STARTING SETTING-UP

        private readonly P3Referential _sharedContext;
        private AccountController _accountController;

        public Integration()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettingsTest.json")
                .Build();

            var _optionsP3Referential = new DbContextOptionsBuilder<P3Referential>()
                .UseSqlServer(configuration.GetConnectionString("P3Referential"))
               .Options;

            var _optionsAppIdentity = new DbContextOptionsBuilder<AppIdentityDbContext>()
                .UseSqlServer(configuration.GetConnectionString("P3Identity"))
                .Options;

            _sharedContext = new P3Referential(_optionsP3Referential, configuration);

            InitializeSeedData();
        }

        public void Dispose()
        {
            _sharedContext?.Dispose();
        }

        public void InitializeSeedData()
        {
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            if (!_sharedContext.Product.Any())
            {
                SeedData.Initialize(serviceProvider, null);
            }
        }

        private static LoginModel StartLoginModel(string username, string password, string returnUrl)
        {
            var loginModel = new LoginModel
            {
                Name = username,
                Password = password,
                ReturnUrl = returnUrl
            };
            return loginModel;
        }

        private static IdentityUser StartIdentityUser(LoginModel loginModel)
        {
            var identityUser = new IdentityUser
            {
                UserName = loginModel.Name
            };
            return identityUser;
        }

        public static bool LoginValidator(LoginModel loginModel)
        {
            return IdentitySeedData.AdminPassword == loginModel.Password && IdentitySeedData.AdminUser == loginModel.Name;
        }

        private void SetupMockingForLoging(LoginModel loginModel, IdentityUser identityUser)
        {
            var mockUserStore = new Mock<IUserStore<IdentityUser>>();
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                mockUserStore.Object, null, null, null, null, null, null, null, null);

            var mockContextAccessor = new Mock<IHttpContextAccessor>();
            var mockUserPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var mockOptions = new Mock<IOptions<IdentityOptions>>();
            var mockLogger = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var mockAuthSchemeProvider = new Mock<IAuthenticationSchemeProvider>();
            var mockUserConfirmation = new Mock<IUserConfirmation<IdentityUser>>();

            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                mockContextAccessor.Object,
                mockUserPrincipalFactory.Object,
                mockOptions.Object,
                mockLogger.Object,
                mockAuthSchemeProvider.Object,
                mockUserConfirmation.Object);

            var mockLocalizer = new Mock<IStringLocalizer<AccountController>>();

            mockLocalizer.Setup(localizer => localizer["Invalid name or password"]).Returns(new LocalizedString("Invalid name or password", "Invalid credentials."));

            bool isValid = LoginValidator(loginModel);

            if (isValid)
            {
                mockUserManager.Setup(um => um.FindByNameAsync(loginModel.Name)).ReturnsAsync(identityUser);
                mockUserManager.Setup(um => um.CheckPasswordAsync(identityUser, loginModel.Password)).ReturnsAsync(true);

                mockSignInManager.Setup(m => m.PasswordSignInAsync(
                    It.IsAny<IdentityUser>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                    .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            }

            if (!isValid)
            {
                mockUserManager.Setup(um => um.FindByNameAsync(loginModel.Name)).ReturnsAsync(identityUser);
                mockUserManager.Setup(um => um.CheckPasswordAsync(identityUser, loginModel.Password)).ReturnsAsync(false);

                mockSignInManager.Setup(m => m.PasswordSignInAsync(
                    It.IsAny<IdentityUser>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                    .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);
            }

            _accountController = new AccountController(
                mockUserManager.Object, mockSignInManager.Object, mockLocalizer.Object);

            var httpContextMock = new Mock<HttpContext>();

            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            };

            mockSignInManager.Object.Context = httpContextMock.Object;
            _accountController.ControllerContext = controllerContext;
        }

        // ENDING SETTING-UP

        // 1. THE DATABASE IS CORRECTLY INITIALIZED
        [Fact]
        private void SeedDateTest()
        {
            foreach (var product in _sharedContext.Product)
            {
                var productInDb = _sharedContext.Product.FirstOrDefault(p => p.Name == product.Name);
                Assert.NotNull(productInDb);
            }
        }

        // 2. THE USER IS CONNECTING TO THE ADMIN PAGE
        [Fact]
        public async Task Login_WithMixedCredentials_ShouldFailThenSucceed()
        {
            // Arrange
            var loginModels = new List<LoginModel>
            {
                StartLoginModel("InvalidUser", "InvalidPassword", null),
                StartLoginModel("Admin", "P@ssword123", null)
            };

            foreach (var loginModel in loginModels)
            {
                IdentityUser identityUser = null;
                if (loginModel.Name == "Admin" && loginModel.Password == "P@ssword123")
                {
                    identityUser = StartIdentityUser(loginModel); // Only create user for valid credentials
                }
                SetupMockingForLoging(loginModel, identityUser);

                bool isValid1 = LoginValidator(loginModel);

                // Act
                var result = await _accountController.Login(loginModel);

                // Assert
                if (isValid1)
                {   // Assertions for Invalid credentials
                    Assert.IsType<RedirectResult>(result);
                    var redirectResult1 = result as RedirectResult;
                    Assert.NotNull(redirectResult1);
                    const string expectedUrl = "/Product/Admin";
                    Assert.Equal(expectedUrl, redirectResult1.Url);
                }
                else
                {   // Assertions for valid credentials
                    Assert.IsType<ViewResult>(result);
                    var viewResult = result as ViewResult;
                    Assert.NotNull(viewResult);
                    Assert.True(viewResult.ViewData.ModelState.ContainsKey("InvalidCredentials"), "ModelState should contain an error for 'InvalidCredentials'");
                }
            }
        }

        // 3. THE USER CREATES TWO NEW PRODUCTS AND DELETE ONE
        [Fact]
        public void AfterLogingCreateAndDeleteOneProductTest()
        {
            Mock<ILanguageService> _mockLanguageService = new Mock<ILanguageService>();

            var productRepository = new ProductRepository(_sharedContext);

            var productService = new ProductService(new Cart(), productRepository, new OrderRepository(_sharedContext), new Mock<IStringLocalizer<ProductService>>().Object);

            var productController = new ProductController(productService, _mockLanguageService.Object);

            ProductViewModel productThatWillBeDeleted = new() // This product is well defined
            {
                Name = "productThatWillBeDeleted",
                Price = "1.00",
                Stock = "666",
                Description = "We create this product. We Assert. We delete. We Assert.",
                Details = "DetailsTest",
            };

            ProductViewModel productThatStaysInDb = new() // This product is well defined
            {
                Name = "productThatStaysInDb",
                Price = "1.00",
                Stock = "666",
                Description = "We create this product. We Assert.",
                Details = "DetailsTest",
            };

            // 3.1 CREATE
            // Act
            var validationContext1 = new ValidationContext(productThatWillBeDeleted, null, null);
            var validationResults1 = new List<ValidationResult>();
            bool isValid2 = Validator.TryValidateObject(productThatWillBeDeleted, validationContext1, validationResults1, true);
            var validationContext2 = new ValidationContext(productThatStaysInDb, null, null);
            var validationResults2 = new List<ValidationResult>();
            bool isValid3 = Validator.TryValidateObject(productThatStaysInDb, validationContext2, validationResults2, true);

            var redirectResult2 = productController.Create(productThatWillBeDeleted) as RedirectToActionResult;
            productController.Create(productThatWillBeDeleted);
            var redirectResult3 = productController.Create(productThatStaysInDb) as RedirectToActionResult;
            productController.Create(productThatStaysInDb);

            Product createdProduct1 = _sharedContext.Product.FirstOrDefault(p => p.Name == productThatWillBeDeleted.Name);
            Product createdProduct2 = _sharedContext.Product.FirstOrDefault(p => p.Name == productThatStaysInDb.Name);

            // Assert
            Assert.True(isValid2, "Model should be valid because every field is well filled");
            Assert.NotNull(redirectResult2);
            Assert.Equal("Admin", redirectResult2.ActionName);
            Assert.NotNull(createdProduct1);

            Assert.True(isValid3, "Model should be valid because every field is well filled");
            Assert.NotNull(redirectResult3);
            Assert.Equal("Admin", redirectResult3.ActionName);
            Assert.NotNull(createdProduct2);

            // 3.2 DELETE
            //Act
            var redirectResult4 = productController.DeleteProduct(productThatWillBeDeleted.Id) as RedirectToActionResult;
            productController.DeleteProduct(productThatWillBeDeleted.Id);
            var deletedProduct1 = _sharedContext.Product.FirstOrDefault(p => p.Id == productThatWillBeDeleted.Id);
            _sharedContext.SaveChanges();
            _sharedContext.ChangeTracker.Clear();

            // Assert
            Assert.NotNull(redirectResult4);
            Assert.Equal("Admin", redirectResult4.ActionName);
            Assert.Null(deletedProduct1);
        }

        // 4. THE USER LOG OUT

        [Fact]
        public async Task WhenClickOnLogoutButton_Logout()
        {
            // Arrange
            LoginModel loginModel = StartLoginModel("Admin", "P@ssword123", null);
            IdentityUser identityUser = StartIdentityUser(loginModel);
            SetupMockingForLoging(loginModel, identityUser);
            await _accountController.Login(loginModel);

            // Act
            RedirectResult result_clickOnLogoutButton_Logout = await _accountController.Logout();

            // Assert
            Assert.IsType<RedirectResult>(result_clickOnLogoutButton_Logout);
            Assert.Equal("/", result_clickOnLogoutButton_Logout.Url);
        }

        [Fact]
        public void CheckIfProductsStillExistInDatabase()
        {
            _sharedContext.SaveChanges();
            _sharedContext.ChangeTracker.Clear();

            var productThatWillBeDeletedCheck = _sharedContext.Product.FirstOrDefault(p => p.Name == "productThatWillBeDeleted");
            var productThatStaysInDbCheck = _sharedContext.Product.FirstOrDefault(p => p.Name == "productThatStaysInDb");

            // Assert that product1 and product2 are not null
            Assert.NotNull(productThatWillBeDeletedCheck);
            Assert.NotNull(productThatStaysInDbCheck);
        }

        // 5. THE USER ADDS TWO PRODUCTS TO HIS CART

        private readonly ILanguageService languageService;

        [Fact]
        public void AddTwoProductsToCart()
        {
            // Arrange
            Cart cart = new();

            var ProductRepository = new ProductRepository(_sharedContext);
            var orderRepository = new OrderRepository(_sharedContext);
            var ProductService = new ProductService(cart, ProductRepository, orderRepository, new Mock<IStringLocalizer<ProductService>>().Object);
            var ProductController = new ProductController(ProductService, languageService);

            CartController cartController = new(cart, ProductService);
            var produtToAdd1 = _sharedContext.Product.FirstOrDefault(p => p.Name == "Echo Dot");
            var produtToAdd2 = _sharedContext.Product.FirstOrDefault(p => p.Name == "productThatWillBeDeleted");
            var produtToAdd3 = _sharedContext.Product.FirstOrDefault(p => p.Name == "productThatStaysInDb");
            Assert.NotNull(produtToAdd1);
            Assert.NotNull(produtToAdd2);

            // Act
            var redirectResult5 = cartController.AddToCart(produtToAdd1.Id) as RedirectToActionResult;
            cartController.AddToCart(produtToAdd1.Id);
            var redirectResult6 = cartController.AddToCart(produtToAdd2.Id) as RedirectToActionResult;
            cartController.AddToCart(produtToAdd2.Id);
            var redirectResult7 = cartController.AddToCart(produtToAdd3.Id) as RedirectToActionResult;
            cartController.AddToCart(produtToAdd3.Id);

            // Assert
            Assert.Equal(3, cart.Lines.Count());
            Assert.Equal(produtToAdd1.Id, cart.Lines.First().Product.Id);
            Assert.Equal(produtToAdd2.Id, cart.Lines.ElementAt(1).Product.Id);
            Assert.Equal(produtToAdd3.Id, cart.Lines.ElementAt(2).Product.Id);
        }

        // 6 . THE USER REMOVES ONE PRODUCT FROM HIS CART
    }
}