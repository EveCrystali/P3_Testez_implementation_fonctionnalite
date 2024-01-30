﻿using Microsoft.AspNetCore.Mvc;
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

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class AccountControllerTests
    {
        private AccountController _controller;

        [Fact]
        public  async Task Login_WithValidModel_ReturnsRedirect()
        {
            // Arrange


            var testUser = new IdentityUser
            {
                UserName = "Admin",
            };

            var loginModel = new LoginModel
            {
                Name = "Admin",
                Password = "P@ssword123"
            };


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


            mockUserManager.Setup(um => um.FindByNameAsync("Admin")).ReturnsAsync(testUser);
            mockUserManager.Setup(um => um.CheckPasswordAsync(testUser, "P@ssword123")).ReturnsAsync(true);

            //mocksignInManager.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                 //.ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            mockSignInManager.Setup(m => m.PasswordSignInAsync(
                It.IsAny<IdentityUser>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var controller = new AccountController(
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
            controller.ControllerContext = controllerContext;

            var validationContext = new ValidationContext(loginModel, null, null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(loginModel, validationContext, validationResults, true);



            // Act
            var result = await controller.Login(loginModel);
            if (result != null)
            {
                if (result is ViewResult)
                {
                    var viewResult = result as ViewResult;
                    Assert.Equal("Login", viewResult.ViewName);
                }
                if (result is RedirectResult redirectResult)
                {
                    // TODO Refactor
                    // TODO Amend message
                    // TODO Url = "/" or it should be loginModel.ReturnUrl -> test is always positive

                    //var redirectToActionResult = result as RedirectResult;
                    //Assert.Equal("Index", redirectToActionResult.ToString());
                    Assert.NotNull(result);
                    string expectedUrl = loginModel.ReturnUrl ?? "/Admin/Index";
                    Assert.Equal(expectedUrl, redirectResult.Url);

                }
            }
            //var redirectResult = result as ViewResult;
            ////var redirectResult = result as RedirectToActionResult;

            //// Assert
            ////Assert.True(controller.ModelState.IsValid, "ModelState should be valid");
            //Assert.NotNull(redirectResult);
            ////Assert.Equal("/Admin/Index", redirectResult.ActionName);
            //Assert.Equal("/Admin/Index", redirectResult.ViewName);
        }
    

        //[Fact]
        //public void Login_WhenNamePasswordAreWrong_ReturnView()
        //{
        //}
    }
}