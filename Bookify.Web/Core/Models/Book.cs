namespace Bookify.Web.Core.Models
{
	public class Book : BaseModel
	{
		public int Id { get; set; }

		[Required, MaxLength(500)]
		public string Title { get; set; }

		// Foreign key for Author
		[Required]
		public int AuthorId { get; set; }
		public Author Author { get; set; }

		[Required, MaxLength(200)]
		public string Publisher { get; set; }

		[Required]
		public DateTime PublishDate { get; set; }

		public string? ImageUrl { get; set; }
		public string? ImageThumbnailUrl { get; set; }

		// Hall property
		[MaxLength(200)]
		public string? Hall { get; set; }

		// Availability for rental
		public bool IsAvailableForRental { get; set; }

		// Description property
		[MaxLength(2000)]
		public string? Description { get; set; }

		// Many-to-Many relationship with Book
		public ICollection<BookCategory> Books { get; set; } = new List<BookCategory>();
	}
}
