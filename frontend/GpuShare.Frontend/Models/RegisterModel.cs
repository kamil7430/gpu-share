using System.ComponentModel.DataAnnotations;
using GpuShare.Frontend.Auth;
using Xunit.Sdk;

namespace GpuShare.Frontend.Models
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = "";

        [Required]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        [MaxLength(20, ErrorMessage = "Username too long")]
        public string Username { get; set; } = "";

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [PasswordComplexity(ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit")]
        public string Password { get; set; } = "";

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = "";

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the Terms and Conditions.")]
        public bool AcceptTerms { get; set; }
    }
}
