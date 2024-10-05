namespace Bookify.Web.DTO
{
    public class AuthorDTO
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Author Name is required")]
        [StringLength(100, ErrorMessage = "Author Name cannot be longer than 100 characters")]
        [Remote(action: "IsAuthorNameUnique", controller: "Authors", AdditionalFields = nameof(Id), ErrorMessage = "Author name already exists.")]
        public string Name { get; set; }

        public bool IsDeleted { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? LastUpdatedOn { get; set; }

    }
}
