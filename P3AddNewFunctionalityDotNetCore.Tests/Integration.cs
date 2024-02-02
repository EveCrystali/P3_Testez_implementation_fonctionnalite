using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class Integration
    {
        // 1. SETTING-UP

        private readonly DbContextOptions<P3Referential> _options;

        public Integration()
        {
            // Configuration de la base de données de test
            _options = new DbContextOptionsBuilder<P3Referential>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            // Initialisation des données de départ
            InitializeSeedData();
        }

        private void InitializeSeedData()
        {
            var serviceCollection = new ServiceCollection();
            // Configurez ici les services nécessaires pour SeedData.Initialize
            serviceCollection.AddDbContext<P3Referential>(options =>
                options.UseInMemoryDatabase("TestDb"));

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using var context = new P3Referential(_options, null);
            if (!context.Product.Any())
            {
                SeedData.Initialize(serviceProvider, null); // Utilisez serviceProvider ici
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

        private void SetupMocking(LoginModel loginModel, IdentityUser identityUser)
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

        //2. THE USER IS CONNECTING TO THE ADMIN PAGE
        [Fact]
        public async Task Login_WithMixedCredentials_ShouldFailThenSucceed()
        {
            // Arrange
            var loginModels = new List<LoginModel>
            {
                StartLoginModel("InvalidUser", "InvalidPassword", null), // First, he uses invalid credentials first
                StartLoginModel("Admin", "P@ssword123", null) // Then he uses valid credentials
            };

            foreach (var loginModel in loginModels)
            {
                IdentityUser identityUser = null;
                if (loginModel.Name == "Admin" && loginModel.Password == "P@ssword123")
                {
                    identityUser = StartIdentityUser(loginModel); // Only create user for valid credentials
                }
                SetupMocking(loginModel, identityUser);

                bool isValid1 = LoginValidator(loginModel);

                // Act
                var result = await _accountController.Login(loginModel);

                // Assert
                if (isValid1)
                {   // Assertions for Invalid credentials
                    Assert.IsType<RedirectResult>(result); // Expect a redirect for valid credentials
                    var redirectResult1 = result as RedirectResult;
                    Assert.NotNull(redirectResult1);
                    const string expectedUrl = "/Product/Admin";
                    Assert.Equal(expectedUrl, redirectResult1.Url);
                }
                else
                {   // Assertions for valid credentials
                    Assert.IsType<ViewResult>(result); // Expect a view result for invalid credentials
                    var viewResult = result as ViewResult;
                    Assert.NotNull(viewResult);
                    Assert.True(viewResult.ViewData.ModelState.ContainsKey("InvalidCredentials"), "ModelState should contain an error for 'InvalidCredentials'");
                }
            }
        }

        private Mock<IProductService> _mockproductService;
        private Mock<ILanguageService> _mockLanguageService;
        private Mock<IStringLocalizer<OrderController>> _mockLocalizer;
        private ProductController _productController;
        private Mock<IServiceProvider> _mockServiceProvider;

        // 3. THE USER CREATES A NEW PRODUCT AND DELETE ONE
        [Fact]
        public void AfterLogingCreateAndDeleteOneProduct()
        {
            _mockLanguageService = new Mock<ILanguageService>();
            _mockLocalizer = new Mock<IStringLocalizer<OrderController>>();

            using var context = new P3Referential(_options, null);

            var mockCart = new Mock<ICart>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockOrderRepository = new Mock<IOrderRepository>();
            var mockLocalizer = new Mock<IStringLocalizer<ProductService>>();
            //var _mockproductService = new Mock<IProductService>();

            var productService = new ProductService(mockCart.Object, mockProductRepository.Object, mockOrderRepository.Object, mockLocalizer.Object);

            _productController = new ProductController(productService, _mockLanguageService.Object);

            var newProduct = new Product { /* initialisation du produit */ };
            mockProductRepository.Setup(repo => repo.SaveProduct(It.IsAny<Product>())).Verifiable();

            ProductViewModel product1 = new() // This product is well defined
            {
                Name = "NameTest",
                Price = "1.00",
                Stock = "1",
                Description = "DescriptionTest",
                Details = "DetailsTest",
                Id = 1
            };

            // Act
            var validationContext1 = new ValidationContext(product1, null, null);
            var validationResults1 = new List<ValidationResult>();
            bool isValid2 = Validator.TryValidateObject(product1, validationContext1, validationResults1, true);
            var redirectResult2 = _productController.Create(product1) as RedirectToActionResult;

            // Assert
            Assert.True(isValid2, "Model should be valid because every field is well filled");
            // TODO :_mockproductService.Verify(service => service.SaveProduct(It.IsAny<ProductViewModel>()), Times.Exactly(1));
            Assert.NotNull(redirectResult2);
            Assert.Equal("Admin", redirectResult2.ActionName);
            // TODO All verifications needed
        }

        //}
    }
}