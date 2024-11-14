namespace Bookify.Web.Core.ViewModels
{
    public class AccountSettingsViewModel : BaseViewModel
    {
        public string? Avatar { get; set; }  // For uploading the avatar image
        public string FullName { get; set; } = string.Empty;  // For the user's full name
        public string PhoneNumber { get; set; } = string.Empty;  // For the user's phone number
        public string? StatusMessage { get; set; }  // For displaying status messages
        public bool ImageRemoved { get; set; }  // To track if the user wants to remove the avatar
    }
}
