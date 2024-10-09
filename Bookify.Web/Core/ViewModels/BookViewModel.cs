using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class BookViewModel : BaseViewModel
    {

        public int Id { get; set; }

        [MaxLength(500, ErrorMessage = Errors.MaxLength)]
        [Remote(action: "IsTitleAuthorUnique", controller: "Books", AdditionalFields = nameof(AuthorId) + "," + nameof(Id), ErrorMessage = Errors.Duplicated)]
        public string Title { get; set; }

        [Display(Name = "Author")]
        [Remote(action: "IsTitleAuthorUnique", controller: "Books", AdditionalFields = nameof(Title) + "," + nameof(Id), ErrorMessage = Errors.Duplicated)]
        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        public string Publisher { get; set; }

        [Required]
        [AssertThat("PublishDate <= Today()", ErrorMessage = Errors.NotAllowFutureDates)]
        public DateTime PublishDate { get; set; } = DateTime.Now;

        //[Remote(action: "ValidateImage", controller: "Books", AdditionalFields = nameof(Id), ErrorMessage = "Invalid image file.")]
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }

        // Hall property
        [MaxLength(50, ErrorMessage = Errors.MaxLength)]
        public string? Hall { get; set; }

        [Display(Name = "Available for rental")]
        public bool IsAvailableForRental { get; set; }

        // Description property
        [MaxLength(2000)]
        public string? Description { get; set; }

        public List<SelectListItem>? Authors { get; set; }

        // Many-to-Many relationship with Book
        public List<SelectListItem>? Categories { get; set; } // For categories
        public List<int> SelectedCategoryIds { get; set; } = new List<int>(); // For multiple selections
    }
}
