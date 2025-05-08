using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class EditClaimDTO
    {
        [EmailAddress]
        [Required]
        public required string Email { get; set; }
    }
}
