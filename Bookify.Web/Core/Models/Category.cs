namespace Bookify.Web.Core.Models;


public class Category : BaseModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = Errors.RequiredField)]
    [StringLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Category")]
    public string Name { get; set; }

}
