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
        public DbSet<Area> Areas { get; set; }
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<RentalCopy> RentalCopies { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Define a sequence for generating serial numbers with a starting value of 1000001
            builder.HasSequence<int>("SerialNumber", schema: "shared")
                .StartsAt(1000001);

            // Set default value for SerialNumber property in BookCopy using the sequence defined above
            builder.Entity<BookCopy>()
                .Property(e => e.SerialNumber)
                .HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");

            // Define a composite key for the BookCategory table (many-to-many relationship between Book and Category)
            builder.Entity<BookCategory>()
                .HasKey(bc => new { bc.BookId, bc.CategoryId });

            builder.Entity<RentalCopy>()
              .HasKey(rc => new { rc.RentalId, rc.BookCopyId });

            // Create a unique index on the Name column in the Category table to ensure each Category name is unique
            builder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Create a unique index on the Email column in the ApplicationUser table (enforcing unique email addresses)
            builder.Entity<ApplicationUser>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Create a unique index on the UserName column in the ApplicationUser table (enforcing unique usernames)
            builder.Entity<ApplicationUser>()
                .HasIndex(c => c.UserName)
                .IsUnique();

            // Create a unique index on the Name column in the Author table to ensure no duplicate author names
            builder.Entity<Author>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Ensure that each combination of Title and AuthorId in the Book table is unique (no duplicate titles for the same author)
            builder.Entity<Book>()
                .HasIndex(b => new { b.Title, b.AuthorId })
                .IsUnique();

            // Ensure that each combination of Name and GovernorateId in the Area table is unique (no duplicate area names within the same governorate)
            builder.Entity<Area>()
                .HasIndex(b => new { b.Name, b.GovernorateId })
                .IsUnique();

            // Create a unique index on the Name column in the Governorate table to ensure no duplicate governorate names
            builder.Entity<Governorate>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Create a unique index on the NationalId column in the Subscriber table to ensure each subscriber has a unique NationalId
            builder.Entity<Subscriber>()
                .HasIndex(c => c.NationalId)
                .IsUnique();

            // Create a unique index on the Email column in the Subscriber table to ensure each subscriber has a unique email address
            builder.Entity<Subscriber>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Create a unique index on the MobileNumber column in the Subscriber table (assuming you want to ensure only one true/false value per subscriber)
            builder.Entity<Subscriber>()
                .HasIndex(c => c.MobileNumber)
                .IsUnique();



            builder.Entity<Rental>().HasQueryFilter(r => !r.IsDeleted);
            builder.Entity<RentalCopy>().HasQueryFilter(r => !r.Rental!.IsDeleted);


            var cascadeFKs = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(builder);
        }
    }
}
