namespace LibraryAPI.DTOs
{
    public class BookWithAuthorDTO: BookDTO
    {
        public List<AuthorDTO> Authors { get; set; } = [];
    }
}
