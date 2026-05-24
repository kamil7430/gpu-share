using System.ComponentModel.DataAnnotations;

namespace GpuShare.Frontend.Auth
{
    public class PasswordComplexityAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var password = value as string;

            if (string.IsNullOrEmpty(password))
                return false;

            return password.Any(char.IsUpper)
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit);
        }
    }
}
