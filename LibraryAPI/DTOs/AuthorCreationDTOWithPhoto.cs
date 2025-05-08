namespace LibraryAPI.DTOs
{
    public class AuthorCreationDTOWithPhoto: AuthorCreationDTO
    {
        // IFormFile Type is used for files representation
        public IFormFile? Photo { get; set; }
    }
}
