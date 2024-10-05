namespace Bookify.Web.Core.Const
{
    public static class Errors
    {
        public const string RequiredField = "{0} Is a Required field";
        public const string MaxLength = "{0} cannot be more than {1} characters";
        public const string MaxMinLength = "The {0} must be at least {1} and at max {2} characters long.";
        public const string Duplicated = "Another record with the same {0} is already exists!";
    }
}
