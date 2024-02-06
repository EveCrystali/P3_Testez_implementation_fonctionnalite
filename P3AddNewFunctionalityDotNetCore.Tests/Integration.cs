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

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class Integration
    {
        // STARTING SETTING-UP

        private readonly DbContextOptions<P3Referential> _optionsP3Referential;
        private readonly DbContextOptions<AppIdentityDbContext> _optionsAppIdentity;

        public Integration()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettingsTest.json")
                .Build();

            _optionsP3Referential = new DbContextOptionsBuilder<P3Referential>()
                .UseSqlServer(configuration.GetConnectionString("P3Referential"))
                .Options;

            _optionsAppIdentity = new DbContextOptionsBuilder<AppIdentityDbContext>()
                .UseSqlServer(configuration.GetConnectionString("P3Identity"))
                .Options;

            InitializeSeedData();
        }

        public void InitializeSeedData()
        {
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            using var context1 = new P3Referential(_optionsP3Referential, null);
            if (!context1.Product.Any())
            {
                SeedData.Initialize(serviceProvider, null);
            }
        }

        private AccountController _accountController;

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
            using var context1 = new P3Referential(_optionsP3Referential, null);

            foreach (var product in context1.Product)
            {
                var productInDb = context1.Product.FirstOrDefault(p => p.Name == product.Name);
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

        private Mock<ILanguageService> _mockLanguageService;

        // 3. THE USER CREATES A NEW PRODUCT AND DELETE ONE
        [Fact]
        public void AfterLogingCreateAndDeleteOneProductTest()
        {
            _mockLanguageService = new Mock<ILanguageService>();

            using var context1 = new P3Referential(_optionsP3Referential, null);
            var productRepository = new ProductRepository(context1);

            var productService = new ProductService(new Cart(), productRepository, new OrderRepository(context1), new Mock<IStringLocalizer<ProductService>>().Object);

            var productController = new ProductController(productService, _mockLanguageService.Object);

            ProductViewModel product1 = new() // This product is well defined
            {
                Name = "NameTest",
                Price = "1.00",
                Stock = "1",
                Description = "We create this product. We Assert. We delete. We Assert",
                Details = "DetailsTest",
            };

            // 3.1 CREATE
            // Act
            var validationContext1 = new ValidationContext(product1, null, null);
            var validationResults1 = new List<ValidationResult>();
            bool isValid2 = Validator.TryValidateObject(product1, validationContext1, validationResults1, true);

            var redirectResult2 = productController.Create(product1) as RedirectToActionResult;
            productController.Create(product1);

            Product createdProduct1 = context1.Product.FirstOrDefault(p => p.Name == product1.Name);

            // Assert
            Assert.True(isValid2, "Model should be valid because every field is well filled");
            Assert.NotNull(redirectResult2);
            Assert.Equal("Admin", redirectResult2.ActionName);
            Assert.NotNull(createdProduct1);

            // 3.2 DELETE
            //Act
            var redirectResult3 = productController.DeleteProduct(product1.Id) as RedirectToActionResult;
            using var context3 = new P3Referential(_optionsP3Referential, null); // New context to refresh cache of Db
            var deletedProduct1 = context3.Product.FirstOrDefault(p => p.Id == product1.Id);

            // Assert
            Assert.NotNull(redirectResult3);
            Assert.Equal("Admin", redirectResult3.ActionName);
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
    }
}