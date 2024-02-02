using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class Integration
    {
        // 1. SETTING-UP

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

            // Mock HttpContext
            var httpContextMock = new Mock<HttpContext>();
            // Set up necessary properties of HttpContext here

            // Mock ControllerContext and assign the mocked HttpContext
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            };

            // Assign ControllerContext to SignInManager and Controller
            mockSignInManager.Object.Context = httpContextMock.Object; // If Context property is available
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

                bool isValid = LoginValidator(loginModel);

                // Act
                var result = await _accountController.Login(loginModel);

                // Assert
                if (isValid)
                {   // Assertions for Invalid credentials
                    Assert.IsType<RedirectResult>(result); // Expect a redirect for valid credentials
                    var redirectResult = result as RedirectResult;
                    Assert.NotNull(redirectResult);
                    const string expectedUrl = "/Product/Admin";
                    Assert.Equal(expectedUrl, redirectResult.Url);
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
    }
}