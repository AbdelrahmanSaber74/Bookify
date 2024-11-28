using Bookify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Application.Common.Interfaces
{
	public interface IApplicationDbContext
	{
		DbSet<Category> Categories { get; set; }
		DbSet<Author> Authors { get; set; }
		DbSet<Book> Books { get; set; }
		DbSet<BookCategory> BookCategories { get; set; }
		DbSet<BookCopy> BookCopies { get; set; }
		DbSet<Area> Areas { get; set; }
		DbSet<Governorate> Governorates { get; set; }
		DbSet<Rental> Rentals { get; set; }
		DbSet<RentalCopy> RentalCopies { get; set; }
		DbSet<Subscriber> Subscribers { get; set; }
		DbSet<Subscription> Subscriptions { get; set; }

		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}
