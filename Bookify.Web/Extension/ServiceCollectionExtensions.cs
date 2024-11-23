using Bookify.Web.Seetings;
using Bookify.Web.Tasks;
using ViewToHTML.Extensions;


namespace Bookify.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Repositories
            services.AddScoped<ICategoriesRepo, CategoriesRepo>()
                    .AddScoped<IAuthorRepo, AuthorRepo>()
                    .AddScoped<IBookRepo, BookRepo>()
                    .AddScoped<IBookCategoryRepo, BookCategoryRepo>()
                    .AddScoped<IBookCopyRepo, BookCopyRepo>()
                    .AddScoped<ISubscribersRepo, SubscribersRepo>()
                    .AddScoped<IGovernorateRepo, GovernorateRepo>()
                    .AddScoped<IAreaRepo, AreaRepo>()
                    .AddScoped<ISubscriptionRepo, SubscriptionRepo>()
                    .AddScoped<IRentalRepo, RentalRepo>()
                    .AddScoped<IRentalCopyRepo, RentalCopyRepo>();



            // Register Email Services
            services.AddTransient<IEmailSender, EmailSender>()
                    .AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();

            services.AddScoped<NotificationService>();
            services.AddScoped<HangfireTasks>();

            // Configure Email Settings
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // Register Security and Data Protection Services
            services.AddDataProtection().SetApplicationName(nameof(Bookify));
            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromSeconds(0);
            });

            // Register Image Service
            services.AddTransient<IImageService, ImageService>();

            // Replace the default ClaimsPrincipalFactory with a custom one
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();

            // Add AutoMapper and load mappings from all assemblies
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Register MVC, Razor Pages, and additional services
            services.AddControllersWithViews()
                    .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    });

            services.AddRazorPages();
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddExpressiveAnnotations();

            return services;
        }
    }
}
