namespace Bookify.Web.Core.Models
{
    
    public class SubscriberFormViewModel : BaseModel
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [MaxLength(100)]
        public string LastName { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        [MaxLength(20)]
        public string NationalId { get; set; } = null!;

        [MaxLength(15)]
        public string MobileNumber { get; set; } = null!;

        public bool HasWhatsApp { get; set; }

        [MaxLength(150)]
        public string Email { get; set; } = null!;

        [MaxLength(500)]
        public string ImageUrl { get; set; } = null!;
        public string ImageThumbnailUrl { get; set; } = null!;

        public int AreaId { get; set; }

 
        public IFormFile? Image { get; set; }
        public int GovernorateId { get; set; }


        [MaxLength(500)]
        public string Address { get; set; } = null!;

        public bool IsBlackListed { get; set; }



        //// List
        public IEnumerable<SelectListItem>? Governorates { get; set; } 
        public IEnumerable<SelectListItem>? Areas { get; set; }

        public int SelectedGovernorateId { get; set; }
        public int SelectedAreaId { get; set; } 

    }
}