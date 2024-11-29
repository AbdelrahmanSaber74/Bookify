using Bookify.Infrastructure;
using Serilog;
using System.Reflection;
using ViewToHTML.Extensions;
using WhatsAppCloudApi.Extensions;
using Bookify.Web.Tasks;
using Bookify.Domain.Coomon;
using Bookify.Web.Core.Maping;
using FluentValidation.AspNetCore;
using Bookify.Web.Validators;

namespace Bookify.Web
{
	public static class ConfigureServices
	{
		public static IServiceCollection AddWebServices(this IServiceCollection services, WebApplicationBuilder builder)
		{
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

			// Configure DbContext
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString!));

			// Add Identity
			services.AddIdentity<ApplicationUser, IdentityRole>(options =>
				options.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultUI()
				.AddDefaultTokenProviders()
				.AddSignInManager<SignInManager<ApplicationUser>>();

			// Application-specific services
			services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

			// Configure Identity options
			services.Configure<IdentityOptions>(options =>
			{
				options.Password.RequiredLength = 8;
				options.User.RequireUniqueEmail = true;
			});

			services.Configure<SecurityStampValidatorOptions>(options =>
				options.ValidationInterval = TimeSpan.Zero);

			// Add additional services
			services.AddDataProtection().SetApplicationName(nameof(Bookify));
			services.AddTransient<IImageService, ImageService>();
			services.AddTransient<IEmailSender, EmailSender>();
			services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();
			services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));
			services.AddWhatsAppApiClient(builder.Configuration);
			services.AddExpressiveAnnotations();
			services.AddViewToHTML();

			// Add Hangfire
			services.AddHangfire(config => config.UseSqlServerStorage(connectionString));
			services.AddHangfireServer();

			// Configure Authorization Policies
			services.Configure<AuthorizationOptions>(options =>
				options.AddPolicy("AdminsOnly", policy =>
				{
					policy.RequireAuthenticatedUser();
					policy.RequireRole(AppRoles.Admin);
				}));

			// Enable Serilog
			services.AddSerilog();

			// Register application services
			services.AddApplicationServices(builder.Configuration);

			return services;
		}
	}
}

namespace Bookify.Web.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
		{
			// Repositories
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

			// Add FluentValidation
			services.AddFluentValidationAutoValidation(); // Enable automatic validation
			services.AddFluentValidationClientsideAdapters(); // Enable client-side adapters for front-end validation
			services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly()); // Register validators in the current assembly


			// Email Services
			services.AddTransient<IEmailSender, EmailSender>()
					.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();

			// Notifications and Tasks
			services.AddScoped<NotificationService>();
			services.AddScoped<HangfireTasks>();

			// Configure Email Settings
			services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

			// Security and Data Protection
			services.AddDataProtection().SetApplicationName(nameof(Bookify));
			services.Configure<SecurityStampValidatorOptions>(options =>
				options.ValidationInterval = TimeSpan.FromSeconds(0));

			// Image Services
			services.AddTransient<IImageService, ImageService>();

			// Claims Principal Factory
			services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();

			// AutoMapper
			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

			// MVC and Razor Pages
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
