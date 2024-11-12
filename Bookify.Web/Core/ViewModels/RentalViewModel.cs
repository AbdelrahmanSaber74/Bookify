    namespace Bookify.Web.Core.ViewModels
{
    public class RentalViewModel
    {
        public int Id { get; set; }
        public string SubscriberKey { get; set; } = null!;
        public SubscriberViewModel? Subscriber { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime CreatedOn { get; set; }
        public bool PenaltyPaid { get; set; }
        public bool IsDeleted { get; set; }
        public IEnumerable<RentalCopyViewModel> RentalCopies { get; set; } = new List<RentalCopyViewModel>();

        public int TotalDelayInDays
        {
            get
            {
                return RentalCopies.Sum(c => c.DelayInDays);
            }
        }

        public int NumberOfCopies
        {
            get
            {
                return RentalCopies.Count();
            }
        }

        public IList<int> SelectedCopies { get; set; } = new List<int>();

        public IEnumerable<BookCopyViewModel> CurrentCopies { get; set; } = new List<BookCopyViewModel>();

        public int? MaxAllowedCopies { get; set; }

    }
}