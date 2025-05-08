using LibraryAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class AuthorPatchDTO
    {
        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(150, ErrorMessage = "The {0} field must to have {1} characters or less")]
        [FirstLetterUpper]
        public required string Names { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(150, ErrorMessage = "The {0} field must to have {1} characters or less")]
        [FirstLetterUpper]
        public required string LastNames { get; set; }

        [StringLength(200, ErrorMessage = "The field {0} must have {1} characters or less")]
        public string? Identification { get; set; }
    }
}
