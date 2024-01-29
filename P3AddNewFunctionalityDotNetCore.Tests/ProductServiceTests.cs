using Microsoft.AspNetCore.Mvc;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using P3AddNewFunctionalityDotNetCore.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using System;
using Microsoft.Extensions.Localization;
using Moq;
using P3AddNewFunctionalityDotNetCore.Models.Services;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class ProductServiceTests
    {

        private Mock<IProductService> _productService;
        private Mock <ILanguageService> _languageService;
        private ProductController _productController;


        [Fact]
        // Tests the scenario where a product is added and it should be added to the list
        public void Create_Add1Product_ProductAddedInList()
        {


        }

        [Fact]
        // Tests the scenario where a product is added with a missing product name and the model state should be invalid
        public void Create_Add1ProductNameMissing_ModelStateInvalid()
        {


        }

        [Fact]
        // Tests the scenario where a product is added with a missing product price and the model state should be invalid
        public void Create_Add1ProductPriceMissing_ModelStateInvalid()
        {


        }

        [Fact]
        // Tests the scenario where a product is added with a non-decimal product price and the model state should be invalid
        public void Create_Add1ProductPriceNotDecimal_ModelStateInvalid()
        {


        }

        [Fact]
        // Tests the scenario where a product is added with a product price not greater than zero and the model state should be invalid
        public void Create_Add1ProductPriceotGreaterThanZero_ModelStateInvalid()
        {


        }

        [Fact]
        // Tests the scenario where a product is added with a missing product quantity and the model state should be invalid
        public void Create_Add1ProductQuantityMissing_ModelStateInvalid()
        {


        }

        [Fact]
        // Tests the scenario where a product is added with a non-numeric product quantity and the model state should be invalid
        public void Create_Add1ProductQuantityNotANumber_ModelStateInvalid()
        {


        }

        [Fact]
        // Tests the scenario where a product is added with a product quantity not greater than zero and the model state should be invalid
        public void Create_Add1ProductQuantityNotGreaterThanZero_ModelStateInvalid()
        {


        }



    }
}