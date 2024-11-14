namespace Bookify.Web.Tasks
{
    public class HangfireTasks
    {
        private readonly NotificationService _notificationService;
        private readonly ISubscriptionRepo _subscriptionRepo;

        public HangfireTasks(NotificationService notificationService, ISubscriptionRepo subscriptionRepo)
        {
            _notificationService = notificationService;
            _subscriptionRepo = subscriptionRepo;
        }

        public async Task SendSubscriptionWarningEmails()
        {
            var subscriptions = await _subscriptionRepo.GetSubscriptionsExpiringInDays(5);

            foreach (var subscription in subscriptions)
            {
                var model = new SubscriberFormViewModel()
                {
                    Email = subscription.Subscriber.Email,
                    FirstName = subscription.Subscriber.FirstName
                };


                await _notificationService.SendWarningEmailForEndOfSubscription(model, subscription.EndDate);

            }
        }

        public static void ConfigureDailyTasks()
        {
            RecurringJob.AddOrUpdate<HangfireTasks>(
                task => task.SendSubscriptionWarningEmails(),
                Cron.Daily
            );
        }
    }
}
