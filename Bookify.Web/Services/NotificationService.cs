using WhatsAppCloudApi;
using WhatsAppCloudApi.Services;

namespace Bookify.Web.Services
{
    public class NotificationService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public NotificationService(
            IWebHostEnvironment webHostEnvironment,
            IWhatsAppClient whatsAppClient,
            IEmailSender emailSender,
            IEmailBodyBuilder emailBodyBuilder)
        {
            _webHostEnvironment = webHostEnvironment;
            _whatsAppClient = whatsAppClient;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }

        public async Task SendWhatsAppNotification(SubscriberFormViewModel model)
        {
            if (model.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>
                {
                    new WhatsAppComponent
                    {
                        Type = "body",
                        Parameters = new List<object>
                        {
                            new WhatsAppTextParameter { Text = $"{model.FirstName}" }
                        }
                    }
                };

                var mobileNumber = _webHostEnvironment.IsDevelopment() ? "01022917856" : model.MobileNumber;

                var result = await _whatsAppClient.SendMessage(
                    $"2{mobileNumber}",
                    WhatsAppLanguageCode.English_US,
                    WhatsAppTemplates.statement_available_2,
                    components
                );

            }
        }

        public async Task SendWelcomeEmail(SubscriberFormViewModel model)
        {
            var placeholder = new Dictionary<string, string>
            {
                { "imageUrl", "https://res.cloudinary.com/dkbsaseyc/image/upload/fl_preserve_transparency/v1729557070/icon-positive-vote-2_jcxdww_rghb1a.jpg?_s=public-apps" },
                { "header", $"Welcome {model.FirstName}," },
                { "body", "Thanks for joining Bookify 🤩" }
            };

            var body = await _emailBodyBuilder.GetEmailBodyAsync(
                EmailTemplates.Notification,
                placeholder
            );

            await _emailSender.SendEmailAsync(model.Email, "Confirm your email", body);
        }

        public async Task SendWarningEmailForEndOfSubscription(SubscriberFormViewModel model, DateTime endDate)
        {
            var placeholder = new Dictionary<string, string>
            {
                { "imageUrl", "https://res.cloudinary.com/dkbsaseyc/image/upload/v1730906414/unnamed_uj1ikj.png" },
                { "header", $"Warning, {model.FirstName}," },
                { "body", $"Your subscription is about to expire in {endDate}. Please renew it to continue enjoying the services" }
            };

            var body = await _emailBodyBuilder.GetEmailBodyAsync(
                EmailTemplates.Notification,
                placeholder
            );

            await _emailSender.SendEmailAsync(model.Email, "Important: Warning Notification", body);
        }


    }
}
