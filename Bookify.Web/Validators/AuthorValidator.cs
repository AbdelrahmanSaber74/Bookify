namespace Bookify.Web.Validators
{
	public class AuthorValidator : AbstractValidator<AuthorViewModel>
	{
		public AuthorValidator() 
		{

			RuleFor(Author => Author.Name)
				.MaximumLength(100)
				.WithMessage(Errors.MaxLength)
				.Matches(RegexPatterns.CharactersOnly_Eng)
				.WithMessage(Errors.OnlyEnglishLetters);

		}

	}
}
