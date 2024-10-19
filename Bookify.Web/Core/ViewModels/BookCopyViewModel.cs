namespace Bookify.Web.Core.ViewModels
{
	public class BookCopyViewModel : BaseViewModel
	{
		public int Id { get; set; }
		public int BookId { get; set; }

		[RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
		public string? BookTitle { get; set; }
		public string? SerialNumber { get; set; }

		[Display(Name = "Available for Rental?")]
		public bool IsAvailableForRental { get; set; }
		public bool IsDelete { get; set; }



		[Display(Name = "Editor Number")]
		[Range(1, 1000, ErrorMessage = Errors.InvalidRange)]
		public int EditorNumber { get; set; }
		public List<SelectListItem>? Books { get; set; }



	}
}
