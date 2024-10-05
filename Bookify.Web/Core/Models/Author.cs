namespace Bookify.Web.Core.Models;


public class Author : BaseModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Author Name is required")]
    [StringLength(100, ErrorMessage = "Author Name cannot be longer than 100 characters")]
    public string Name { get; set; }

    // Navigation property for Books
    public ICollection<Book>? Books { get; set; }
}
