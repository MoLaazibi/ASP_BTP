using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AP.BTP.UI.Attributes
{
    public class PasswordStrengthAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || value is not string password)
            {
                return new ValidationResult("Wachtwoord is verplicht.");
            }

            if (password.Length < 8)
            {
                return new ValidationResult("Wachtwoord moet minimaal 8 tekens lang zijn.");
            }

            int characterTypes = 0;

            // Check for lowercase letters
            if (Regex.IsMatch(password, @"[a-z]"))
                characterTypes++;

            // Check for uppercase letters
            if (Regex.IsMatch(password, @"[A-Z]"))
                characterTypes++;

            // Check for numbers
            if (Regex.IsMatch(password, @"[0-9]"))
                characterTypes++;

            // Check for special characters
            if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                characterTypes++;

            if (characterTypes < 3)
            {
                return new ValidationResult("Wachtwoord moet minimaal 3 van de volgende 4 typen bevatten: kleine letters (a-z), hoofdletters (A-Z), cijfers (0-9), speciale tekens.");
            }

            return ValidationResult.Success;
        }
    }
}

