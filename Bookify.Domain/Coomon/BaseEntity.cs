namespace Bookify.Domain.Coomon
{
    public class BaseEntity
    {
        public bool IsDeleted { get; set; }

        public string? CreatedById { get; set; }

        public ApplicationUser? CreatedBy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public string? LastUpdatedById { get; set; }

        public ApplicationUser? LastUpdatedBy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? LastUpdatedOn { get; set; }
    }
}
