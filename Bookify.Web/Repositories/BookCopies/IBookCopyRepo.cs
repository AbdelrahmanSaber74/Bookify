namespace Bookify.Web.Repositories.BookCopies
{
	public interface IBookCopyRepo
	{

		Task<List<BookCopy>> GetBookCopiesByBookIdAsync(int bookId);
        Task<BookCopy> GetCopyDetails(string serialNumber);
        Task<BookCopy> GetBookCopyByIdAsync(int id);
        Task DeleteBookCopyByIdAsync(int id);
		Task<BookCopy> UpdateBookCopyAsync(BookCopy bookCopy);
		Task<BookCopy> AddBookCopyAsync(BookCopy book);
	}
}
