using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Linq;
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
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests.UnitTests
{
    public class AccountControllerTests
    {
        private AccountController _accountController;
        private Mock<SignInManager<IdentityUser>> mockSignInManager;

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

            mockSignInManager = new Mock<SignInManager<IdentityUser>>(
            mockUserManager.Object,
            mockContextAccessor.Object,
            mockUserPrincipalFactory.Object,
            mockOptions.Object,
            mockLogger.Object,
            mockAuthSchemeProvider.Object,
            mockUserConfirmation.Object);

            var mockLocalizer = new Mock<IStringLocalizer<AccountController>>();

            mockLocalizer.Setup(localizer => localizer["Invalid name or password"]).Returns(new LocalizedString("Invalid name or password", "Invalid credentials."));

            if (LoginValidator(loginModel))
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

            if (!LoginValidator(loginModel))
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

        [Fact]
        public async Task Login_WhenValidCredentials_LogsInAndRedirectsToAdminIndex()
        {
            // ARRANGE

            LoginModel loginModel = StartLoginModel("Admin", "P@ssword123", null);
            IdentityUser identityUser = StartIdentityUser(loginModel);
            SetupMocking(loginModel, identityUser);

            // ACT
            var result = await _accountController.Login(loginModel);

            // ASSERT

            if (result != null && LoginValidator(loginModel))
            {
                if (result is ViewResult viewResult)
                {
                    Assert.Equal("Login", viewResult.ViewName);
                }
                else if (result is RedirectResult redirectResult)
                {
                    Assert.NotNull(result);
                    const string expectedUrl = "/";
                    Assert.Equal(expectedUrl, redirectResult.Url);
                }
            }
            else
            {
                Assert.Fail("Invalid name or password");
            }
        }

        [Fact]
        public async Task Login_WhenInvalidCredentials_NotLogIn()
        {
            // Arrange
            var loginModels = new List<LoginModel>
            {
                StartLoginModel("WrongId", "WrongPassword", null),
                StartLoginModel("WrongId", "P@ssword123", null),
                StartLoginModel("Admin", "WrongPassword", null),
                StartLoginModel("", "", null),
                StartLoginModel("", "P@ssword123", null),
                StartLoginModel("Admin", "", null)
            };
            var identityUsers = loginModels.ConvertAll(StartIdentityUser);
            foreach (var indexedUser in loginModels.Select((model, index) => new { Model = model, Index = index }))
            {
                SetupMocking(indexedUser.Model, identityUsers[indexedUser.Index]);
            }

            // Act & Assert
            foreach (var loginModel in loginModels)
            {
                // Act
                var result = await _accountController.Login(loginModel);
                var viewResult = result as ViewResult;

                //Assert
                Assert.False(LoginValidator(loginModel), "ModelState should be Invalid because we have used wrong credentials");
                Assert.NotNull(result);
                Assert.IsType<ViewResult>(result);
                Assert.True(viewResult.ViewData.ModelState.ContainsKey("InvalidCredentials"), "ModelState should contain an error for 'InvalidCredentials'");
            }
        }

        [Fact]
        public async Task Logout_WhenUserClickOnLogout_LogoutAndRedirect()
        {
            LoginModel loginModel = StartLoginModel("Admin", "P@ssword123", null);
            IdentityUser identityUser = StartIdentityUser(loginModel);
            SetupMocking(loginModel, identityUser);
            mockSignInManager.Setup(m => m.SignOutAsync()).Returns(Task.CompletedTask);

            // Act
            var loginResult = await _accountController.Login(loginModel);
            mockSignInManager.Invocations.Clear(); // Clear previous invocations
            var redirectResult = await _accountController.Logout() as RedirectResult;

            // Assert
            mockSignInManager.Verify(m => m.SignOutAsync(), Times.Once);
            Assert.IsType<RedirectResult>(redirectResult);
            Assert.Equal("/", redirectResult.Url);
        }
    }
}