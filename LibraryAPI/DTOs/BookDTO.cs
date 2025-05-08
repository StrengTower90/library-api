using LibraryAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class BookDTO
    {
        //[Required]
        //[StringLength(150, ErrorMessage = "The {0} field must to have {1} characters or less")]
        public int Id { get; set; }
        public required string Title { get; set; }
    }
}
