using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class BookCreationDTO
    {
        [Required]
        [StringLength(150, ErrorMessage = "The {0} field must to have {1} characters or less")]
        public required string Title { get; set; }
        public List<int> AuthorIds { get; set; } = [];
    }
}
