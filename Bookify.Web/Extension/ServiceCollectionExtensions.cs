using Bookify.Web.Repositories.BookCopies;

namespace Bookify.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register services here
            services.AddScoped<ICategoriesRepo, CategoriesRepo>();

            services.AddScoped<IAuthorRepo, AuthorRepo>();

            services.AddScoped<IBookRepo, BookRepo>();

            services.AddScoped<IBookCategoryRepo, BookCategoryRepo>();

            services.AddScoped<IBookCopyRepo, BookCopyRepo>();



			// Replace the default ClaimsPrincipalFactory with the custom one
			services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();

			// Register AutoMapper in the DI container and load mappings from the current application domain's assemblies
			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            return services;
        }
    }
}
