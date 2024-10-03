

using System.ComponentModel.DataAnnotations;

namespace SharedClassLibrary.Models; 

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category Name is required")]
    [StringLength(100, ErrorMessage = "Category Name cannot be longer than 100 characters")]
    public string Name { get; set; }

    public bool IsDeleted { get; set; } // Soft delete indicator

    [DataType(DataType.DateTime)]
    public DateTime CreatedOn { get; set; } = DateTime.Now; // Tracks creation time

    [DataType(DataType.DateTime)]
    public DateTime? LastUpdatedOn { get; set; }  // Tracks last update time
}
