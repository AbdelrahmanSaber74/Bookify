using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Bookify.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.HasSequence<int>("SerialNumber", schema: "shared")
                .StartsAt(1000001);

            builder.Entity<BookCopy>()
                .Property(e => e.SerialNumber)
                .HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");

            builder.Entity<BookCategory>()
                .HasKey(bc => new { bc.BookId, bc.CategoryId }); // Define composite key


            // Create a unique index on the Name column in the Category table.
            builder.Entity<Category>().
                HasIndex(c => c.Name)
                .IsUnique();


            // Create a unique index on the Email column in the ApplicationUser table.
            builder.Entity<ApplicationUser>().
                HasIndex(c => c.Email)
                .IsUnique();

            // Create a unique index on the UserName column in the ApplicationUser table.
            builder.Entity<ApplicationUser>().
                HasIndex(c => c.UserName)
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
