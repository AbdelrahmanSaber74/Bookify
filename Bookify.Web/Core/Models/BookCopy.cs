namespace Bookify.Web.Core.Models;


public class BookCopy : BaseModel
{
	public int Id { get; set; }
	public int BookId { get; set; }
	public Book? Book { get; set; }
	public bool IsAvailableForRental { get; set; }
	public bool IsDelete { get; set; }
	public int EditorNumber { get; set; }
	public int SerialNumber { get; set; }

}
