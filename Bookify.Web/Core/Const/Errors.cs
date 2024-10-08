namespace Bookify.Web.Core.Const
{
    public static class Errors
    {
        public const string RequiredField = "{0} is a required field.";
        public const string MaxLength = "{0} cannot be more than {1} characters.";
        public const string MaxMinLength = "The {0} must be at least {1} and at max {2} characters long.";
        public const string Duplicated = "Another record with the same {0} already exists!";
        public const string DuplicatedBook = "Book with the same title is already exists with the same author!";

        // Add image-related error messages
        public const string NoFileProvided = "No file provided.";
        public const string FileSizeExceeded = "File size exceeds the maximum limit of {0} MB.";
        public const string InvalidFileType = "Invalid file type. Only {0} are allowed.";
        public const string PermissionDenied = "You do not have permission to save this file.";
        public const string SaveError = "An error occurred while saving the file. Please try again.";
        public const string UnexpectedError = "An unexpected error occurred while saving the file.";
    }
}
