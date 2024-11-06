namespace Bookify.Web.Repositories.Subscription

{
    public interface ISubscriptionRepo
    {
        Task<Bookify.Web.Core.Models.Subscription> GetLastSubscriptionBySubscriberId(int subscriberId);
        Task UpdateSubscription(Bookify.Web.Core.Models.Subscription subscription);
        Task AddSubscription(Bookify.Web.Core.Models.Subscription subscription);
        Task<IEnumerable<Bookify.Web.Core.Models.Subscription>> GetSubscriptionsExpiringInDays(int daysBeforeEndDate);

    }
}
