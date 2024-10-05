using Bookify.Web.Core.Models;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Create a unique index on the Name column in the Category table.
            builder.Entity<Category>().
				HasIndex(c => c.Name)
				.IsUnique();


            base.OnModelCreating(builder);
        }

    }
}
