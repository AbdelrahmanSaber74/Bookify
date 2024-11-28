namespace Bookify.Web.Repositories.Subscription
{
    public interface ISubscriptionRepo
    {
        Task<Bookify.Domain.Entities.Subscription> GetLastSubscriptionBySubscriberId(int subscriberId);
        Task UpdateSubscription(Bookify.Domain.Entities.Subscription subscription);
        Task AddSubscription(Bookify.Domain.Entities.Subscription subscription);
        Task<IEnumerable<Bookify.Domain.Entities.Subscription>> GetSubscriptionsExpiringInDays(int daysBeforeEndDate);

    }
}
