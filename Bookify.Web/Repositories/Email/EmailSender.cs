using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Bookify.Web.Seetings;

namespace Bookify.Web.Repositories.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
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

                mailMessage.To.Add(email);

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
