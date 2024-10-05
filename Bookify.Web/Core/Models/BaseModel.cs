namespace Bookify.Web.Core.Models;


public class BaseModel
{

    public bool IsDeleted { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedOn { get; set; } = DateTime.Now;

    [DataType(DataType.DateTime)]
    public DateTime? LastUpdatedOn { get; set; }
}
