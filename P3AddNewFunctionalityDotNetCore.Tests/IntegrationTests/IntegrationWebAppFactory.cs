using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests.IntegrationTests
{
    public class IntegrationWebApplicationFactory : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public IntegrationWebApplicationFactory(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            var clientOptions = new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("http://localhost:60700")
            };
            _client = _factory.CreateClient(clientOptions);
        }

        private static string ExtractAntiForgeryToken(string htmlContent)
        {
            var match = Regex.Match(htmlContent, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
            return match.Success ? match.Groups[1].Value : null;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Account/Login")]
        [InlineData("/Cart/")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task Test_LoginFailsWithWrongCredentials()
        {
            // Arrange
            const string expectedErrorMessage = "Invalid name or password"; // Define the expected error message
            var loginPageResponse = await _client.GetAsync("/Account/Login");
            var loginPageContent = await loginPageResponse.Content.ReadAsStringAsync();
            var antiforgeryToken = ExtractAntiForgeryToken(loginPageContent);

            FormUrlEncodedContent loginContent1 = new(new[]
                {
                    new KeyValuePair<string, string>("Name", ""),
                    new KeyValuePair<string, string>("Password", ""),
                    new KeyValuePair<string, string>("__RequestVerificationToken", antiforgeryToken)
                });

            FormUrlEncodedContent loginContent2 = new(new[]
                {
                    new KeyValuePair<string, string>("Name", "BadUser"),
                    new KeyValuePair<string, string>("Password", "BadPassword"),
                    new KeyValuePair<string, string>("__RequestVerificationToken", antiforgeryToken)
                });

            List<FormUrlEncodedContent> loginContents = new()
            {
                loginContent1,
                loginContent2
            };

            foreach (FormUrlEncodedContent loginContent in loginContents)
            {
                // Act
                var loginResponse = await _client.PostAsync("/Account/Login", loginContent);

                // Assert

                if (loginResponse.StatusCode == HttpStatusCode.Redirect) // Log in
                {
                    Assert.Fail("Should not be redirected here with bad credentials");
                }
                else if (loginResponse.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await loginResponse.Content.ReadAsStringAsync();
                    Assert.Contains(expectedErrorMessage, responseContent);
                }
                else
                {
                    Assert.Fail("Case not handled in the login method - Unexpected errors");
                }
            }
        }

        [Fact]
        public async Task Test_LoginSuccessThenLogout()
        {
            // Arrange
            var loginPageResponse = await _client.GetAsync("/Account/Login");
            var loginPageContent = await loginPageResponse.Content.ReadAsStringAsync();
            var antiforgeryToken = ExtractAntiForgeryToken(loginPageContent);

            var loginContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Name", "Admin"),
                new KeyValuePair<string, string>("Password", "P@ssword123"),
                new KeyValuePair<string, string>("__RequestVerificationToken", antiforgeryToken)
            });

            // Act
            var loginResponse = await _client.PostAsync("/Account/Login", loginContent);

            // Assert
            if (loginResponse.StatusCode == HttpStatusCode.Redirect) // Log in
            {
                Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
                var redirectUrl = loginResponse.Headers.Location.ToString();
                Assert.Equal("/", redirectUrl);
                var followRedirectResponse = await _client.GetAsync(redirectUrl);
                Assert.Equal(HttpStatusCode.OK, followRedirectResponse.StatusCode);
            }
            else if (loginResponse.StatusCode == HttpStatusCode.OK)
            {
                Assert.Fail("Should not be here with good credentials");
            }
            else
            {
                Assert.Fail("Case not handled in the login method - Unexpected errors");
            }

            // Act
            //Logout
            var logoutResponse = await _client.PostAsync("/Account/Logout", null);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, logoutResponse.StatusCode);
            Assert.Equal("/", logoutResponse.Headers.Location?.OriginalString);
        }
    }
}