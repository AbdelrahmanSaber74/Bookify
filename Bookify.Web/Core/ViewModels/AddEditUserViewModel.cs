namespace Bookify.Web.Core.ViewModels
{
    public class AddEditUserViewModel
    {
        public string? Id { get; set; } = null!;

        [Display(Name ="Full Name")]
        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        [RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        public string? FullName { get; set; } = null;

        [MaxLength(20 , ErrorMessage = Errors.MaxLength)]
        public string? UserName { get; set; } = null;

        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        [EmailAddress(ErrorMessage = Errors.InvalidEmailFormat)]
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; } = null;

        [StringLength(100, ErrorMessage = Errors.MaxMinLength  , MinimumLength = 6)]
        [RegularExpression(RegexPatterns.Password  ,  ErrorMessage = Errors.WeakPassword)] 
        public string Password { get; set; } = null!;
        [Compare("Password", ErrorMessage = Errors.PasswordMismatchMessage)]
        public string ConfirmPassword { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastUpdatedOn { get; set; }

        // Role properties

        public IEnumerable<SelectListItem> Roles = new List<SelectListItem>();
        public List<string> SelectedRoles { get; set; } = new List<string>();  

    }
}
