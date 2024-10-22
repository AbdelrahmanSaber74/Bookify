
using System.Text.Encodings.Web;

namespace Bookify.Web.Services
{
	public class EmailBodyBuilder : IEmailBodyBuilder
	{
		private readonly IWebHostEnvironment _webHostEnvironment;

		public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment )
		{
			_webHostEnvironment = webHostEnvironment;
		}

		public async Task<string> GetEmailBodyAsync(string imageUrl, string header, string bodyContent, string linkTitle , string callBack)
		{
			// Load the HTML template from file
			var filePath = $"{_webHostEnvironment.WebRootPath}/templates/email.html";	
			string body;

			using (var streamReader = new StreamReader(filePath))
			{
				body = await streamReader.ReadToEndAsync();
			}

			// Replace placeholders in the email template
			body = body.Replace("[imageUrl]", imageUrl)
					   .Replace("[header]", header)
					   .Replace("[body]", bodyContent)
					   .Replace("[url]", HtmlEncoder.Default.Encode(callBack))
					   .Replace("[linkTitle]", linkTitle);

			return body;
		}
	}
}
