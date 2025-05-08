using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Entities
{
    public class Comment
    {
        /* We use Guid because int only allow 2,000 millions of id's 
         * and Guid Allow more than that, and we expect to have more comments */
        public Guid Id { get; set; }
        [Required]
        public required string Body { get; set; }
        public DateTime PublicationDate { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public required string UserId { get; set; }
        public bool IsDeleted { get; set; }
        public User? User { get; set; }
    }
}
