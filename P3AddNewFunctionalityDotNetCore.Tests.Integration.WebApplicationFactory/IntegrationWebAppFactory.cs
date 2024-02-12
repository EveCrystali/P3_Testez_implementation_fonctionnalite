using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
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
    public class IntegrationWebApplicationFactory : IClassFixture<WebApplicationFactory<Program>>

    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;


        public IntegrationWebApplicationFactory(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Index")]
        [InlineData("/Product/Admin")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task TestLoginAndLogout()
        {
            // Simule une requête de login
            var contenuLogin = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", "Admin"),
                new KeyValuePair<string, string>("password", "P@ssword123")
            });

            var loginResponse = await _client.PostAsync("/login", contenuLogin);

            // Vérifie que le login a réussi
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            // Simule une requête de logout
            var logoutResponse = await _client.PostAsync("/", null);

            // Vérifie que le logout a réussi
            Assert.Equal(HttpStatusCode.Redirect, logoutResponse.StatusCode);
        }



    }

}