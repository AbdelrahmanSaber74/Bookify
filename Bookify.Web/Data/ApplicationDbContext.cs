using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Bookify.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {



            builder.Entity<BookCategory>()
                .HasKey(bc => new { bc.BookId, bc.CategoryId }); // Define composite key


            // Create a unique index on the Name column in the Category table.
            builder.Entity<Category>().
                HasIndex(c => c.Name)
                .IsUnique();


            // Create a unique index on the Name column in the Author table.
            builder.Entity<Author>().
                HasIndex(c => c.Name)
                .IsUnique();

            // Ensure Title and AuthorId are unique together
            builder.Entity<Book>()
                .HasIndex(b => new { b.Title, b.AuthorId })
                .IsUnique();



            base.OnModelCreating(builder);
        }

    }
}
