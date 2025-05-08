using LibraryAPI.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Data
{
    //public class ApplicationDbContext : DbContext 
    //instead of inherit from DbContext now we gonna inherit from IdentityDbContext
    public class ApplicationDbContext: IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global filter that affect every query
            modelBuilder.Entity<Comment>().HasQueryFilter(b => !b.IsDeleted);
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<AuthorBook> AuthorsBooks { get; set; }
        public DbSet<Error> Errors { get; set; }
    }
}
