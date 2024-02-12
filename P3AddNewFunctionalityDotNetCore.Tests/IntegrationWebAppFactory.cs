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
using System.Net.Http;
using Xunit;
using System.Text.RegularExpressions;


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

            // Cr�e un nouveau client HTTP avec gestion des cookies
            var clientOptions = new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true, // Pour suivre les redirections automatiquement
                BaseAddress = new Uri("http://localhost:60700") // Cela peut ne pas �tre n�cessaire avec WebApplicationFactory
            };
            var client = _factory.CreateClient(clientOptions);

        }

        private string ExtractAntiForgeryToken(string htmlContent)
        {
            // Utilise une expression r�guli�re ou un analyseur HTML pour extraire le token
            // Par exemple, tu peux utiliser HtmlAgilityPack ou une autre biblioth�que pour analyser le HTML
            // et r�cup�rer la valeur du champ cach� __RequestVerificationToken.

            // Ceci est un exemple et doit �tre adapt� pour ton cas sp�cifique :
            var match = Regex.Match(htmlContent, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
            return match.Success ? match.Groups[1].Value : null;
        }


        [Theory]
        [InlineData("/")]
        [InlineData("/Product/Create")]
        [InlineData("Account/Login?ReturnUrl=%2FProduct%2FAdmin")]
        [InlineData("/Cart/")]
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
            //// Simule une requ�te de login
            //var contenuLogin = new FormUrlEncodedContent(new[]
            //{
            //    new KeyValuePair<string, string>("Username", "Admin"),
            //    new KeyValuePair<string, string>("Password", "P@ssword123")
            //});

            //var loginResponse = await _client.PostAsync("/Account/Login?ReturnUrl=%2FProduct%2FAdmin", contenuLogin);

            //// V�rifie que le login a r�ussi
            //Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            //if (loginResponse.StatusCode == HttpStatusCode.Redirect)
            //{
            //    var redirectUrl = loginResponse.Headers.Location.ToString();
            //    var followRedirectResponse = await _client.GetAsync(redirectUrl);
            //    Assert.Equal(HttpStatusCode.OK, followRedirectResponse.StatusCode);
            //}


            // Faire une requ�te GET � la page de login pour r�cup�rer le token anti-falsification
            var getLoginResponse = await _client.GetAsync("/Account/Login");
            getLoginResponse.EnsureSuccessStatusCode();
            var getLoginContent = await getLoginResponse.Content.ReadAsStringAsync();

            // Extraire le token anti-falsification � partir du contenu de la page
            var antiForgeryToken = ExtractAntiForgeryToken(getLoginContent);

            // V�rifier que le token a �t� trouv�
            Assert.NotNull(antiForgeryToken);

            // Construire le contenu de la requ�te POST pour inclure les informations de connexion et le token
            var loginContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("userName", "Admin"),
                new KeyValuePair<string, string>("password", "P@ssword123"),
                new KeyValuePair<string, string>("__RequestVerificationToken", antiForgeryToken)
            });

            // Envoyer la requ�te POST � l'endpoint de login
            var loginResponse = await _client.PostAsync("/Account/Login?ReturnUrl=%2FProduct%2FAdmin", loginContent);

            // V�rifie que le login a r�ussi et a retourn� une redirection vers la page d'administration
            if (loginResponse.StatusCode == HttpStatusCode.Redirect)
            {
                // Connexion r�ussie, v�rifier la redirection
                Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
                var redirectUrl = loginResponse.Headers.Location.ToString();
                Assert.Equal("/Product/Admin", redirectUrl); // Ou l'URL exacte attendue

                // Suivre la redirection et v�rifier la page de destination
                var followRedirectResponse = await _client.GetAsync(redirectUrl);
                Assert.Equal(HttpStatusCode.OK, followRedirectResponse.StatusCode);
            }
            else if (loginResponse.StatusCode == HttpStatusCode.OK)
            {
                // Connexion �chou�e, v�rifier le contenu de la r�ponse
                var responseContent = await loginResponse.Content.ReadAsStringAsync();
                // V�rifier ici si le contenu contient le message d'erreur
                //Assert.Contains(_localizer["Invalid name or password"], responseContent);
                Assert.Fail(responseContent);
            }


        }



    }

}