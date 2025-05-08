using LibraryAPI.Validations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Entities
{
    public class Author
    {
        public int Id { get; set; }

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
        [Unicode(false)]
        public string? Photo { get; set; }
        public List<AuthorBook> Books { get; set; } = new List<AuthorBook>();
    }
}
