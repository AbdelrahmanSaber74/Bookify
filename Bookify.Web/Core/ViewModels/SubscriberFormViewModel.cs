using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.Models
{
    
    public class SubscriberFormViewModel : BaseModel
    {
        public int Id { get; set; }

        [MaxLength(100), Display(Name = "First Name"),
        RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
        public string FirstName { get; set; } = null!;

        [MaxLength(100), Display(Name = "Last Name"),
            RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
        public string LastName { get; set; } = null!;

        [Display(Name = "Date Of Birth")]
        [AssertThat("DateOfBirth <= Today()", ErrorMessage = Errors.NotAllowFutureDates)]
        public DateTime? DateOfBirth { get; set; } = DateTime.Now;

        [MaxLength(14), Display(Name = "National ID"),
            RegularExpression(RegexPatterns.NationalId, ErrorMessage = Errors.InvalidNationalId)]
        [Remote(controller: "Subscribers", action: "AllowNationalId", AdditionalFields = nameof(Id), ErrorMessage = Errors.Duplicated)]

        public string NationalId { get; set; } = null!;

        [MaxLength(11), Display(Name = "Mobile Number"),
            RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
        [Remote(controller: "Subscribers", action: "AllowMobileNumber", AdditionalFields = nameof(Id), ErrorMessage = Errors.Duplicated)]

        public string MobileNumber { get; set; } = null!;
        public bool HasWhatsApp { get; set; }
        [MaxLength(150)]
        [EmailAddress]
        [Remote(controller: "Subscribers", action: "AllowEmail", AdditionalFields = nameof(Id), ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;


        [RequiredIf("Id == 0", ErrorMessage = Errors.EmptyImage)]
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; } = null!;
        public string? ImageThumbnailUrl { get; set; } = null!;

        [MaxLength(500)]
        public string Address { get; set; } = null!;

        public bool IsBlackListed { get; set; }


        public IEnumerable<SelectListItem>? Governorates { get; set; } 
        public IEnumerable<SelectListItem>? Areas { get; set; }

        public int GovernorateId { get; set; }
        public int AreaId { get; set; } 

    }
}