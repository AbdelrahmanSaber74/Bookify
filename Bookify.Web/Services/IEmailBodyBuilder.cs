namespace Bookify.Web.Services
{
    public interface IEmailBodyBuilder
    {
        Task<string> GetEmailBodyAsync(string template, Dictionary<string, string> placeholders);
    }
}
