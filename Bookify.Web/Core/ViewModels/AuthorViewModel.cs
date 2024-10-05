namespace Bookify.Web.Core.ViewModels
{
    public class AuthorViewModel : BaseViewModel
    {

        public int Id { get; set; }

        [Display(Name = "Author")]
        [Required(ErrorMessage = Errors.RequiredField)]
        [StringLength(100, ErrorMessage = Errors.MaxLength)]

        [Remote(action: "IsAuthorNameUnique", controller: "Authors", AdditionalFields = nameof(Id), ErrorMessage = Errors.Duplicated)]
        public string Name { get; set; }

    }
}
