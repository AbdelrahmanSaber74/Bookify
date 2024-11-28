namespace Bookify.Infrastructure
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
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

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return await base.SaveChangesAsync(cancellationToken);
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			ConfigureSequences(builder);
			ConfigureEntityRelationships(builder);
			ConfigureIndexes(builder);
			ApplyGlobalQueryFilters(builder);
			SetCascadeDeleteBehavior(builder);
		}

		private static void ConfigureSequences(ModelBuilder builder)
		{
			builder.HasSequence<int>("SerialNumber", schema: "shared")
				.StartsAt(1000001);

			builder.Entity<BookCopy>()
				.Property(e => e.SerialNumber)
				.HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");
		}

		private static void ConfigureEntityRelationships(ModelBuilder builder)
		{
			builder.Entity<BookCategory>()
				.HasKey(bc => new { bc.BookId, bc.CategoryId });

			builder.Entity<RentalCopy>()
				.HasKey(rc => new { rc.RentalId, rc.BookCopyId });
		}

		private static void ConfigureIndexes(ModelBuilder builder)
		{
			builder.Entity<Category>()
				.HasIndex(c => c.Name)
				.IsUnique();

			builder.Entity<ApplicationUser>()
				.HasIndex(c => c.Email)
				.IsUnique();

			builder.Entity<ApplicationUser>()
				.HasIndex(c => c.UserName)
				.IsUnique();

			builder.Entity<Author>()
				.HasIndex(c => c.Name)
				.IsUnique();

			builder.Entity<Book>()
				.HasIndex(b => new { b.Title, b.AuthorId })
				.IsUnique();

			builder.Entity<Area>()
				.HasIndex(a => new { a.Name, a.GovernorateId })
				.IsUnique();

			builder.Entity<Governorate>()
				.HasIndex(g => g.Name)
				.IsUnique();

			builder.Entity<Subscriber>()
				.HasIndex(s => s.NationalId)
				.IsUnique();

			builder.Entity<Subscriber>()
				.HasIndex(s => s.Email)
				.IsUnique();

			builder.Entity<Subscriber>()
				.HasIndex(s => s.MobileNumber)
				.IsUnique();
		}

		private static void ApplyGlobalQueryFilters(ModelBuilder builder)
		{
			builder.Entity<Rental>().HasQueryFilter(r => !r.IsDeleted);
			builder.Entity<RentalCopy>().HasQueryFilter(rc => !rc.Rental!.IsDeleted);
		}

		private static void SetCascadeDeleteBehavior(ModelBuilder builder)
		{
			var cascadeFKs = builder.Model.GetEntityTypes()
				.SelectMany(t => t.GetForeignKeys())
				.Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

			foreach (var fk in cascadeFKs)
			{
				fk.DeleteBehavior = DeleteBehavior.Restrict;
			}
		}
	}
}
