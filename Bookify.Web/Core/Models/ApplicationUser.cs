namespace Bookify.Web.Core.Models
{
    public class ApplicationUser : IdentityUser
    {

        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        public bool IsDeleted { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? LastUpdatedOn { get; set; }
        public string? CreatedById { get; set; }
        public string? LastUpdatedById { get; set; }
        public string? ImageUrl {  get; set; }
        public string? ThumbnailUrl { get; set; }

    }
}
