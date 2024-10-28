﻿
using Bookify.Web.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Web.Repositories.Subscription
{
    public class SubscriptionRepo : ISubscriptionRepo
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddSubscription(Core.Models.Subscription subscription)
        {
            await _context.Subscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync();
        }


        public async Task<Core.Models.Subscription> GetLastSubscriptionBySubscriberId(int subscriberId)
        {

            return await _context.Subscriptions
                                 .Where(m => m.SubscriberId == subscriberId)
                                 .OrderByDescending(m => m.EndDate)
                                 .FirstOrDefaultAsync(); 
        }

        public async Task UpdateSubscription(Core.Models.Subscription subscription)
        {
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
        }

    }
}

