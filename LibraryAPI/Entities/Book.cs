using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAPI.Entities
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "The {0} field must to have {1} characters or less")]
        public required string Title { get; set; }
        public List<AuthorBook> Authors { get; set; } = [];
        public List<Comment> Comments { get; set; } = [];
    }
}
