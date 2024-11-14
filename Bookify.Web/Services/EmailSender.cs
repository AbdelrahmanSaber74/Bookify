using Bookify.Web.Seetings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Bookify.Web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailSender(IOptions<EmailSettings> emailSettings, IWebHostEnvironment webHostEnvironment)
        {
            _emailSettings = emailSettings.Value;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (var smtpClient = new SmtpClient(_emailSettings.SmtpServer))
            {
                smtpClient.Port = _emailSettings.Port;
                smtpClient.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                smtpClient.EnableSsl = _emailSettings.EnableSSL;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };



                mailMessage.To.Add(_webHostEnvironment.IsDevelopment() ? "tech.abdelrahman.s@gmail.com" : email);

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (SmtpException smtpEx)
                {
                    throw new InvalidOperationException($"SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error sending email: {ex.Message}", ex);
                }
            }
        }
    }
}
