using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests.IntegrationTests
{
    public class Integration : IDisposable
    {
        // STARTING SETTING-UP

        public ProductRepository productRepository;
        public readonly P3Referential _sharedContext;
        public AccountController _accountController;
        public SignInManager<IdentityUser> SignInManager;

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
            productRepository = new ProductRepository(_sharedContext);

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

        private void SetupMockingForLogIn(LoginModel loginModel, IdentityUser identityUser)
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
        private void SeedDataTest()
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
                SetupMockingForLogIn(loginModel, StartIdentityUser(loginModel));

                // Act
                var result = await _accountController.Login(loginModel);

                // Assert
                if (LoginValidator(loginModel))
                {   // Assertions for Invalid credentials
                    Assert.IsType<RedirectResult>(result);
                    var redirectResult1 = result as RedirectResult;
                    Assert.NotNull(redirectResult1);
                    const string expectedUrl = "/";
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

        public static void ValidateProduct(ProductViewModel product)
        {
            var validationContext = new ValidationContext(product, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);
            Assert.True(isValid, "Model should be valid because every field is well filled");
        }

        private static void CheckProductFields(Product createdProduct, ProductViewModel productViewModel)
        {
            Assert.Equal(createdProduct.Name, productViewModel.Name);
            var actualPrice = decimal.Parse(createdProduct.Price.ToString(CultureInfo.InvariantCulture));
            var expectedPrice = decimal.Parse(productViewModel.Price, CultureInfo.InvariantCulture);
            Assert.Equal(expectedPrice, actualPrice);
            Assert.Equal(productViewModel.Stock, createdProduct.Quantity.ToString());
            Assert.Equal(createdProduct.Description, productViewModel.Description);
            Assert.Equal(createdProduct.Details, productViewModel.Details);
        }

        // 3. THE USER CREATES TWO NEW PRODUCTS AND DELETE ONE
        [Fact]
        public void AfterLogingInCreateAndDeleteOneProductTest()
        {
            Mock<ILanguageService> _mockLanguageService = new();

            var productService = new ProductService(new Cart(), productRepository, new OrderRepository(_sharedContext), new Mock<IStringLocalizer<ProductService>>().Object);

            var productController = new ProductController(productService, _mockLanguageService.Object);

            ProductViewModel productViewModel1 = new() // This product is well defined
            {
                Name = "DeleteThis",
                Price = "1.00",
                Stock = "666",
                Description = "We create this product. We Assert. We delete. We Assert.",
                Details = "DetailsTest",
            };

            ProductViewModel productViewModel2 = new() // This product is well defined
            {
                Name = "KeepThis",
                Price = "1.00",
                Stock = "666",
                Description = "We create this product. We Assert.",
                Details = "DetailsTest",
            };

            // 3.1 CREATE
            // Act
            ValidateProduct(productViewModel1);
            ValidateProduct(productViewModel2);
            productController.Create(productViewModel1);
            productController.Create(productViewModel2);

            Product createdProduct1 = _sharedContext.Product.FirstOrDefault(p => p.Name == productViewModel1.Name);
            Product createdProduct2 = _sharedContext.Product.FirstOrDefault(p => p.Name == productViewModel2.Name);

            // Assert
            Assert.NotNull(createdProduct1);
            Assert.NotNull(createdProduct2);
            _sharedContext.SaveChanges();
            _sharedContext.ChangeTracker.Clear();
            // Refetch the product from the database just before checking
            _sharedContext.SaveChanges();
            _sharedContext.ChangeTracker.Clear();
            Product refreshedProduct1 = _sharedContext.Product.FirstOrDefault(p => p.Id == createdProduct1.Id);
            Product refreshedProduct2 = _sharedContext.Product.FirstOrDefault(p => p.Id == createdProduct2.Id);
            CheckProductFields(refreshedProduct1, productViewModel1);
            CheckProductFields(refreshedProduct2, productViewModel2);

            // 3.2 User deletes one product
            //Act
            var redirectResult3 = productController.DeleteProduct(createdProduct1.Id) as RedirectToActionResult;
            _sharedContext.SaveChanges();
            _sharedContext.ChangeTracker.Clear();

            // Assert
            Assert.Equal("Admin", redirectResult3.ActionName);
            Assert.Null(_sharedContext.Product.FirstOrDefault(p => p.Id == createdProduct1.Id));
            Dispose();
        }

        // 4. THE USER LOG OUT

        [Fact]
        public async Task LogOut_WhenUserClickOnLogout_LogoutAndRedirect()
        {
            //// Arrange
            LoginModel loginModel = StartLoginModel("Admin", "P@ssword123", null);
            IdentityUser identityUser = StartIdentityUser(loginModel);
            SetupMockingForLogIn(loginModel, identityUser);
            await _accountController.Login(loginModel); // Integration approach spirit

            // Act
            RedirectResult result_clickOnLogoutButton_Logout = await _accountController.Logout();

            // Assert
            Assert.IsType<RedirectResult>(result_clickOnLogoutButton_Logout);
            Assert.Equal("/", result_clickOnLogoutButton_Logout.Url);
        }
    }
}