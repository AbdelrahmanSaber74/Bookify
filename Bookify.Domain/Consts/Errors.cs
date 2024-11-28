namespace Bookify.Domain.Consts
{
    public static class Errors
    {
        // General validation errors
        public const string RequiredField = "{0} is a required field.";
        public const string MaxLength = "{0} cannot be more than {1} characters.";
        public const string MinLength = "{0} must be at least {1} characters long.";
        public const string MaxMinLength = "The {0} must be at least {1} and at max {2} characters long.";
        public const string InvalidRange = "{0} should be between {1} and {2}!";

        // Duplication and uniqueness errors
        public const string Duplicated = "Another record with the same {0} already exists!";
        public const string DuplicatedBook = "Book with the same title already exists with the same author!";

        // Date validation errors
        public const string NotAllowFutureDates = "Date cannot be in the future!";

        // Password validation errors
        public const string PasswordMismatchMessage = "The password and confirmation password do not match!";
        public const string WeakPassword = "Passwords must contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long.";

        // Email and phone validation errors
        public const string InvalidEmailFormat = "The email format is invalid.";
        public const string InvalidMobileNumber = "Invalid mobile number.";

        // Image-related errors
        public const string ImageSaveFailed = "Failed to save the image. Please try again.";
        public const string EmptyImage = "Please select an image.";
        public const string NoFileProvided = "No file provided.";
        public const string FileSizeExceeded = "File size exceeds the maximum limit of {0} MB.";
        public const string InvalidFileType = "Invalid file type. Only {0} are allowed.";
        public const string PermissionDenied = "You do not have permission to save this file.";
        public const string SaveError = "An error occurred while saving the file. Please try again.";
        public const string UnexpectedError = "An unexpected error occurred while saving the file.";

        // National ID and serial number validation errors
        public const string InvalidNationalId = "Invalid national ID.";
        public const string InvalidSerialNumber = "Invalid serial number.";

        // Subscriber errors
        public const string BlackListedSubscriber = "This subscriber is blacklisted.";
        public const string InactiveSubscriber = "This subscriber is inactive.";
        public const string MaxCopiesReached = "This subscriber has reached the max number for rentals.";
        public const string SubscriberNotFound = "Subscriber not found.";

        // Book rental-related errors
        public const string BookCopyUnavailable = "The specified book copy or its main book is not available for rental.";
        public const string RentalSuccessMessage = "Rental has been successfully created!";
        public const string BookAlreadyRented = "This book copy has already been rented by you.";
        public const string InvalidSubscriberId = "The subscriber ID is invalid. Please check the link or contact support.";
        public const string CopyIsInRental = "This copy is already rentaled.";

        // Additional validation and restrictions
        public const string InvalidUsername = "Username can only contain letters or digits.";
        public const string OnlyEnglishLetters = "Only English letters are allowed.";
        public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
        public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
        public const string DenySpecialCharacters = "Special characters are not allowed.";

        // File and extension validation
        public const string NotAllowedExtension = "Only .png, .jpg, .jpeg files are allowed!";
        public const string MaxSize = "File cannot be more than 2 MB!";

        // Rental-specific errors
        public const string NotAvilableRental = "This book/copy is not available for rental.";
        public const string RentalNotAllowedForBlacklisted = "Rental cannot be extended for blacklisted subscribers.";
        public const string RentalNotAllowedForInactive = "Rental cannot be extended for this subscriber before renewal.";
        public const string ExtendNotAllowed = "Rental cannot be extended.";
        public const string PenaltyShouldBePaid = "Penalty should be paid.";
        public const string InvalidStartDate = "Invalid start date.";
        public const string InvalidEndDate = "Invalid end date.";
    }
}
