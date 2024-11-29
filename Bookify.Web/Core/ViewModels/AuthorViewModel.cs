namespace Bookify.Web.Core.ViewModels
{
    public class AuthorViewModel : BaseViewModel
    {

        public int Id { get; set; }

        [Display(Name = "Author")]
        [Required(ErrorMessage = Errors.RequiredField)]
        [Remote(action: "IsAuthorNameUnique", controller: "Authors", AdditionalFields = nameof(Id), ErrorMessage = Errors.Duplicated)]
        public string Name { get; set; }

    }
}
