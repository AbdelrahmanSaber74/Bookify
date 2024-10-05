namespace Bookify.Web.Core.ViewModels
{
    public class CategoryViewModel : BaseViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Category")]
        [Required(ErrorMessage = Errors.RequiredField)]
        [StringLength(100, ErrorMessage = Errors.MaxLength)]
        [Remote(action: "IsCategoryNameUnique", controller: "Categories", AdditionalFields = nameof(Id), ErrorMessage = Errors.Duplicated)]
        public string Name { get; set; }
    }
}