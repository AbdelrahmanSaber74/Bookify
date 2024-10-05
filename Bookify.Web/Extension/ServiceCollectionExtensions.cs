using Bookify.Web.Repositories.Categories;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;

namespace Bookify.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register services here
            services.AddScoped<ICategoriesRepo, CategoriesRepo>();




            // Register AutoMapper in the DI container and load mappings from the current application domain's assemblies
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            return services;
        }
    }
}
