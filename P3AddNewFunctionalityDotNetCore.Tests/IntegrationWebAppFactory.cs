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

            // Crée un nouveau client HTTP avec gestion des cookies
            var clientOptions = new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true, // Pour suivre les redirections automatiquement
                BaseAddress = new Uri("http://localhost:60700") // Cela peut ne pas être nécessaire avec WebApplicationFactory
            };
            var client = _factory.CreateClient(clientOptions);

        }

        private string ExtractAntiForgeryToken(string htmlContent)
        {
            // Utilise une expression régulière ou un analyseur HTML pour extraire le token
            // Par exemple, tu peux utiliser HtmlAgilityPack ou une autre bibliothèque pour analyser le HTML
            // et récupérer la valeur du champ caché __RequestVerificationToken.

            // Ceci est un exemple et doit être adapté pour ton cas spécifique :
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
            //// Simule une requête de login
            //var contenuLogin = new FormUrlEncodedContent(new[]
            //{
            //    new KeyValuePair<string, string>("Username", "Admin"),
            //    new KeyValuePair<string, string>("Password", "P@ssword123")
            //});

            //var loginResponse = await _client.PostAsync("/Account/Login?ReturnUrl=%2FProduct%2FAdmin", contenuLogin);

            //// Vérifie que le login a réussi
            //Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            //if (loginResponse.StatusCode == HttpStatusCode.Redirect)
            //{
            //    var redirectUrl = loginResponse.Headers.Location.ToString();
            //    var followRedirectResponse = await _client.GetAsync(redirectUrl);
            //    Assert.Equal(HttpStatusCode.OK, followRedirectResponse.StatusCode);
            //}


            // Faire une requête GET à la page de login pour récupérer le token anti-falsification
            var getLoginResponse = await _client.GetAsync("/Account/Login");
            getLoginResponse.EnsureSuccessStatusCode();
            var getLoginContent = await getLoginResponse.Content.ReadAsStringAsync();

            // Extraire le token anti-falsification à partir du contenu de la page
            var antiForgeryToken = ExtractAntiForgeryToken(getLoginContent);

            // Vérifier que le token a été trouvé
            Assert.NotNull(antiForgeryToken);

            // Construire le contenu de la requête POST pour inclure les informations de connexion et le token
            var loginContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("userName", "Admin"),
                new KeyValuePair<string, string>("password", "P@ssword123"),
                new KeyValuePair<string, string>("__RequestVerificationToken", antiForgeryToken)
            });

            // Envoyer la requête POST à l'endpoint de login
            var loginResponse = await _client.PostAsync("/Account/Login?ReturnUrl=%2FProduct%2FAdmin", loginContent);

            // Vérifie que le login a réussi et a retourné une redirection vers la page d'administration
            if (loginResponse.StatusCode == HttpStatusCode.Redirect)
            {
                // Connexion réussie, vérifier la redirection
                Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
                var redirectUrl = loginResponse.Headers.Location.ToString();
                Assert.Equal("/Product/Admin", redirectUrl); // Ou l'URL exacte attendue

                // Suivre la redirection et vérifier la page de destination
                var followRedirectResponse = await _client.GetAsync(redirectUrl);
                Assert.Equal(HttpStatusCode.OK, followRedirectResponse.StatusCode);
            }
            else if (loginResponse.StatusCode == HttpStatusCode.OK)
            {
                // Connexion échouée, vérifier le contenu de la réponse
                var responseContent = await loginResponse.Content.ReadAsStringAsync();
                // Vérifier ici si le contenu contient le message d'erreur
                //Assert.Contains(_localizer["Invalid name or password"], responseContent);
                Assert.Fail(responseContent);
            }


        }



    }

}