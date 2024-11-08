namespace Bookify.Web.Core.ViewModels
{
    public class RentalFormViewModel
    {
        public string SubscriberKey { get; set; } = null!;
        public BookCopyViewModel BookCopyDetails { get; set; }

        public int BookCopyId { get; set; }
        public IEnumerable<Book> Books { get; set; } 

    }
}