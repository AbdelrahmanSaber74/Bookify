using Bookify.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

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


			services.AddTransient<IEmailSender, EmailSender>();

			services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();




			services.Configure<SecurityStampValidatorOptions>(options =>
			{
				options.ValidationInterval = TimeSpan.FromSeconds(0);
			});

			// Register ImageService with DI container
			services.AddTransient<IImageService, ImageService>();

			// Replace the default ClaimsPrincipalFactory with the custom one
			services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();

			// Register AutoMapper in the DI container and load mappings from the current application domain's assemblies
			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

			return services;
		}
	}
}
