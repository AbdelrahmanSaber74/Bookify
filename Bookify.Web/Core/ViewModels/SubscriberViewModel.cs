namespace Bookify.Web.Core.Models
{
    
    public class SubscriberViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string? Key { get; set; }

        public string FullName { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        public string NationalId { get; set; } = null!;

        public string MobileNumber { get; set; } = null!;

        public bool HasWhatsApp { get; set; }

        public string Email { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        public string ImageThumbnailUrl { get; set; } = null!;

        public int AreaId { get; set; }

        public Area? Area { get; set; }

        public int GovernorateId { get; set; }

        public Governorate? Governorate { get; set; }

        public string Address { get; set; } = null!;

        public bool IsBlackListed { get; set; }

        public IEnumerable<SubscriptionViewModel> Subscriptions { get; set; } = new List<SubscriptionViewModel>();
    }
}