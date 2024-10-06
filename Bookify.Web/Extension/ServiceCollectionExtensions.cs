using Bookify.Web.Repositories.Authors;
using Bookify.Web.Repositories.Books;
using Bookify.Web.Repositories.BooksCategories;
using Bookify.Web.Repositories.Categories;

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




            // Register AutoMapper in the DI container and load mappings from the current application domain's assemblies
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            return services;
        }
    }
}
