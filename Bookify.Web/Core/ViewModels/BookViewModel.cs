using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Core.ViewModels
{
    public class BookViewModel : BaseViewModel
    {

        public int Id { get; set; }

        [MaxLength(500 , ErrorMessage = Errors.MaxLength)]
        public string Title { get; set; }

        [Display(Name ="Author")]
        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        public string Publisher { get; set; }

        [Required]
        [Display(Name = "Publish Date")]
        public DateTime PublishDate { get; set; } = DateTime.Now;

        public IFormFile? ImageUrl { get; set; }

        // Hall property
        [MaxLength(50, ErrorMessage = Errors.MaxLength)]
        public string? Hall { get; set; }

        [Display(Name = "Available for rental")]
        public bool IsAvailableForRental { get; set; }

        // Description property
        [MaxLength(2000)]
        public string? Description { get; set; }

        // Many-to-Many relationship with Book
        public List<SelectListItem>? Authors { get; set; }
        public List<SelectListItem>? Categories { get; set; } // For categories
        public List<int>? SelectedCategoryIds { get; set; } = new List<int>(); // For multiple selections
    }
}
