using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Validations
{
    public class FirstLetterUpperAttribute: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var valueString = value.ToString()!; // "!" to indicate that the value doesn't be null
            var firstLetter = valueString[0].ToString();

            if (firstLetter != firstLetter.ToUpper())
            {
                return new ValidationResult("The first letter must be capitalize");
            }

            return ValidationResult.Success;
        }
    }
}
