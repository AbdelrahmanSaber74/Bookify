namespace Bookify.Web.Services
{
	public interface IEmailBodyBuilder
	{
		 Task<string> GetEmailBodyAsync(string imageUrl, string header, string bodyContent, string linkTitle, string callBack);
	}
}
