namespace Bookify.Web.Core.ViewModels
{
    public class BaseViewModel
    {
        [Required(ErrorMessage = Errors.RequiredField)]
        [Display(Name ="Status")]
        public bool IsDeleted { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? LastUpdatedOn { get; set; }

    }
}
