using Bookify.Web.Repositories.Categories;
using Microsoft.CodeAnalysis.Host;
using Microsoft.Extensions.DependencyInjection;

namespace Bookify.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register services here
            services.AddScoped<ICategoriesRepo, CategoriesRepo>();

            return services;
        }
    }
}
