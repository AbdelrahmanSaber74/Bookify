
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

		public async Task<string> GetEmailBodyAsync(string template, Dictionary<string , string > placeholders)
		{
			// Load the HTML template from file	
			var filePath = $"{_webHostEnvironment.WebRootPath}/templates/{template}.html";	
			string body; 

			using (var streamReader = new StreamReader(filePath))
			{
				body = await streamReader.ReadToEndAsync();
			}

            // Replace placeholders in the email template
            foreach (var placeholder in placeholders)
            {
                // Encoding the values to prevent any potential HTML injection
                var encodedValue = HtmlEncoder.Default.Encode(placeholder.Value);
                body = body.Replace($"[{placeholder.Key}]", encodedValue);
            }

            return body;
		}
	}
}
