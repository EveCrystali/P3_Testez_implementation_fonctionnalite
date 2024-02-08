using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System;
using Xunit;
using Microsoft.Extensions.Localization;
using Moq;
using System.Linq;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using System.Drawing;
using System.Security.Cryptography.Xml;
using P3AddNewFunctionalityDotNetCore.Data;

namespace P3AddNewFunctionalityDotNetCore.Tests.UnitTests
{
    public class AccountControllerTests
    {
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

        [Fact]
        public async Task Login_WhenValidCredentials_LogsInAndRedirectsToAdminIndex()
        {
            // ARRANGE

            LoginModel loginModel = StartLoginModel("Admin", "P@ssword123", null);
            IdentityUser identityUser = StartIdentityUser(loginModel);
            SetupMocking(loginModel, identityUser);

            bool isValid = LoginValidator(loginModel);

            // ACT
            var result = await _accountController.Login(loginModel);

            // ASSERT

            if (result != null && isValid)
            {
                if (result is ViewResult viewResult)
                {
                    Assert.Equal("Login", viewResult.ViewName);
                }
                else if (result is RedirectResult redirectResult)
                {
                    Assert.NotNull(result);
                    const string expectedUrl = "/Product/Admin";
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
            // ARRANGE - Different scenarios of invalid credentials

            // Username: WrongId, Password: WrongPassword
            LoginModel loginModel = StartLoginModel("WrongId", "WrongPassword", null);
            IdentityUser identityUser = StartIdentityUser(loginModel);
            SetupMocking(loginModel, identityUser);

            // Username: "WrongId", Password: "P@ssword123"
            LoginModel loginModel2 = StartLoginModel("WrongId", "P@ssword123", null);
            IdentityUser identityUser2 = StartIdentityUser(loginModel2);
            SetupMocking(loginModel2, identityUser2);

            // Username: "Admin", Password: "WrongPassword"
            LoginModel loginModel3 = StartLoginModel("Admin", "WrongPassword", null);
            IdentityUser identityUser3 = StartIdentityUser(loginModel3);
            SetupMocking(loginModel3, identityUser3);

            // Username: "", Password: ""
            LoginModel loginModel4 = StartLoginModel("", "", null);
            IdentityUser identityUser4 = StartIdentityUser(loginModel4);
            SetupMocking(loginModel4, identityUser4);

            // Username: "", Password: "P@ssword123"
            LoginModel loginModel5 = StartLoginModel("", "P@ssword123", null);
            IdentityUser identityUser5 = StartIdentityUser(loginModel5);
            SetupMocking(loginModel5, identityUser5);

            // Username: "Admin", Password: ""
            LoginModel loginModel6 = StartLoginModel("Admin", "", null);
            IdentityUser identityUser6 = StartIdentityUser(loginModel6);
            SetupMocking(loginModel6, identityUser6);

            bool isValid = LoginValidator(loginModel);
            bool isValid2 = LoginValidator(loginModel2);
            bool isValid3 = LoginValidator(loginModel3);
            bool isValid4 = LoginValidator(loginModel4);
            bool isValid5 = LoginValidator(loginModel5);
            bool isValid6 = LoginValidator(loginModel6);

            // ACT
            var result = await _accountController.Login(loginModel);
            var viewResult = result as ViewResult;

            var result2 = await _accountController.Login(loginModel2);
            var viewResult2 = result2 as ViewResult;

            var result3 = await _accountController.Login(loginModel3);
            var viewResult3 = result3 as ViewResult;

            var result4 = await _accountController.Login(loginModel4);
            var viewResult4 = result4 as ViewResult;

            var result5 = await _accountController.Login(loginModel5);
            var viewResult5 = result5 as ViewResult;

            var result6 = await _accountController.Login(loginModel6);
            var viewResult6 = result6 as ViewResult;

            // ASSERT

            Assert.False(isValid, "ModelState should be Invalid because we have used wrong credentials");
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ModelState.ContainsKey("InvalidCredentials"), "ModelState should contain an error for 'InvalidCredentials'");

            Assert.False(isValid2, "ModelState should be Invalid because we have used wrong credentials");
            Assert.NotNull(result2);
            Assert.IsType<ViewResult>(result2);
            Assert.True(viewResult2.ViewData.ModelState.ContainsKey("InvalidCredentials"), "ModelState should contain an error for 'InvalidCredentials'");

            Assert.False(isValid3, "ModelState should be Invalid because we have used wrong credentials");
            Assert.NotNull(result3);
            Assert.IsType<ViewResult>(result3);
            Assert.True(viewResult3.ViewData.ModelState.ContainsKey("InvalidCredentials"), "ModelState should contain an error for 'InvalidCredentials'");

            Assert.False(isValid4, "ModelState should be Invalid because we have used wrong credentials");
            Assert.NotNull(result4);
            Assert.IsType<ViewResult>(result4);
            Assert.True(viewResult4.ViewData.ModelState.ContainsKey("InvalidCredentials"), "ModelState should contain an error for 'InvalidCredentials'");

            Assert.False(isValid5, "ModelState should be Invalid because we have used wrong credentials");
            Assert.NotNull(result5);
            Assert.IsType<ViewResult>(result5);
            Assert.True(viewResult5.ViewData.ModelState.ContainsKey("InvalidCredentials"), "ModelState should contain an error for 'InvalidCredentials'");

            Assert.False(isValid6, "ModelState should be Invalid because we have used wrong credentials");
            Assert.NotNull(result6);
            Assert.IsType<ViewResult>(result6);
            Assert.True(viewResult6.ViewData.ModelState.ContainsKey("InvalidCredentials"), "ModelState should contain an error for 'InvalidCredentials'");
        }
    }
}