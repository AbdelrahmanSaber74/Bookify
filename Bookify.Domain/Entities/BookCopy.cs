namespace Bookify.Domain.Entities;
public class BookCopy : BaseEntity
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public bool IsAvailableForRental { get; set; }
    public bool IsDelete { get; set; }
    public int EditorNumber { get; set; }
    public int SerialNumber { get; set; }

}
