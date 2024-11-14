namespace Bookify.Web.Core.Models;


public class Category : BaseModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = Errors.RequiredField)]
    [StringLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Category")]
    public string Name { get; set; }

    // Many-to-Many relationship with Book
    public ICollection<BookCategory> Categories { get; set; } = new List<BookCategory>();

}
