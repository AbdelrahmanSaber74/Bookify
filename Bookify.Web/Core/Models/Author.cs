namespace Bookify.Web.Core.Models;


public class Author
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Author Name is required")]
    [StringLength(100, ErrorMessage = "Author Name cannot be longer than 100 characters")]
    public string Name { get; set; }

    public bool IsDeleted { get; set; } 

    [DataType(DataType.DateTime)]
    public DateTime CreatedOn { get; set; } = DateTime.Now; 

    [DataType(DataType.DateTime)]
    public DateTime? LastUpdatedOn { get; set; }
}
