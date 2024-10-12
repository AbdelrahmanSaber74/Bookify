namespace Bookify.Web.Core.ViewModels
{
    public class BookCopyViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string? BookTitle { get; set; }
        public bool IsAvailableForRental { get; set; }
        public bool IsDelete { get; set; }
        public int EditorNumber { get; set; }
        public int SerialNumber { get; set; }

    }
}
