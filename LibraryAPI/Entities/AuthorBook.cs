using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Entities
{
    /* Through this way we get the unicity */
    [PrimaryKey(nameof(AuthorId), nameof(BookId))]
    public class AuthorBook
    {
        public int AuthorId { get; set; }
        public int BookId { get; set; }
        public int Order { get; set; }
        public Author? Author { get; set; }
        public Book? Book { get; set; }
    }
}
