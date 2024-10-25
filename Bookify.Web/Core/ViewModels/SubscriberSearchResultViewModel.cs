namespace Bookify.Web.Core.Models
{
    public class SubscriberSearchResultViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string NationalId { get; set; }
        public string MobileNumber { get; set; }
        public bool HasWhatsApp { get; set; }
        public string Email { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public int AreaId { get; set; }
        public int GovernorateId { get; set; }
        public string Address { get; set; }
        public bool IsBlackListed { get; set; }
    }
}
