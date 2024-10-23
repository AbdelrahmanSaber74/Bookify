namespace Bookify.Web.Core.Consts
{
	public static class Errors
	{
		public const string RequiredField = "{0} is a required field.";
		public const string MaxLength = "{0} cannot be more than {1} characters.";
		public const string MinLength = "{0} must be at least {1} characters long.";
		public const string MaxMinLength = "The {0} must be at least {1} and at max {2} characters long.";
		public const string Duplicated = "Another record with the same {0} already exists!";
		public const string DuplicatedBook = "Book with the same title already exists with the same author!";
		public const string NotAllowFutureDates = "Date cannot be in the future!";
		public const string InvalidRange = "{0} should be between {1} and {2}!";
		public const string PasswordMismatchMessage = "The password and confirmation password do not match!";
		public const string InvalidEmailFormat = "The email format is invalid.";
		public const string WeakPassword = "Passwords must contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long.";
		public const string InvalidMobileNumber = "Invalid mobile number.";
        public const string ImageSaveFailed = "Failed to save the image. Please try again.";
        public const string InvalidNationalId = "Invalid national ID.";
        public const string EmptyImage = "Please select an image.";

        // Add image-related error messages
        public const string NoFileProvided = "No file provided.";
		public const string FileSizeExceeded = "File size exceeds the maximum limit of {0} MB.";
		public const string InvalidFileType = "Invalid file type. Only {0} are allowed.";
		public const string PermissionDenied = "You do not have permission to save this file.";
		public const string SaveError = "An error occurred while saving the file. Please try again.";
		public const string UnexpectedError = "An unexpected error occurred while saving the file.";

		// Additional messages from the second class
		public const string OnlyEnglishLetters = "Only English letters are allowed.";
		public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
		public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
		public const string DenySpecialCharacters = "Special characters are not allowed.";
		public const string NotAllowedExtension = "Only .png, .jpg, .jpeg files are allowed!";
		public const string MaxSize = "File cannot be more than 2 MB!";
	}
}
