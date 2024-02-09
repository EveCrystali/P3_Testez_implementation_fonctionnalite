using System.ComponentModel.DataAnnotations;

namespace P3AddNewFunctionalityDotNetCore.Models.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "ErrorMissingName")]
        public string Name { get; set; }

        [Required(ErrorMessage = "ErrorMissingPassword")]
        public string Password { get; set; }

        public string? ReturnUrl { get; set; } = null ;
    }
}