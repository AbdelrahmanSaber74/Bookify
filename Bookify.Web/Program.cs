using Bookify.Web.Tasks;
using Hangfire.Dashboard;
using Serilog;
using Serilog.Context;
using ViewToHTML.Extensions;
using WhatsAppCloudApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure the connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add database context services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Identity services with UserManager and RoleManager
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequiredLength = 8; // Set minimum password length
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

// Configure Hangfire to use SQL Server for task storage
builder.Services.AddHangfire(config => config.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();


builder.Services.AddViewToHTML();
// Add Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // From appsettings.json
    .CreateLogger();
builder.Host.UseSerilog();

// Register additional services
builder.Services.AddWhatsAppApiClient(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Create a service scope to initialize roles and admin users
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Seed default roles and admin user
    await SeedRoles.AddDefaultRolesAsync(roleManager);
    await DefaultUser.SeedAdminUser(userManager);
}

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler(errorHandlingPath: "/Home/Error");
    app.UseHsts();
}

app.UseExceptionHandler(errorHandlingPath: "/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error", "?StatusCode={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY"); 
    await next();
});


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configure Hangfire dashboard
app.UseHangfireDashboard("/Hangfire", new DashboardOptions
{
    AppPath = "/home",
    DashboardTitle = "My Custom Hangfire Dashboard",
    StatsPollingInterval = 1000,
    IgnoreAntiforgeryToken = false,
    IsReadOnlyFunc = (DashboardContext context) => true,
    Authorization = new IDashboardAuthorizationFilter[]
    {
        new HangfireAuthorizationFilter()
    }

});

// Register daily task execution 
app.Lifetime.ApplicationStarted.Register(() =>
{
    using (var scope = app.Services.CreateScope())
    {
        HangfireTasks.ConfigureDailyTasks();
    }
});


// Middleware to add UserId and UserName to LogContext
app.Use(async (context, next) =>
{
    // Get user details from HttpContext or Authentication system (placeholder logic)
    string userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
    string userName = context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";

    // Add values to Serilog's LogContext
    LogContext.PushProperty("UserId", userId);
    LogContext.PushProperty("UserName", userName);

    await next();

});

app.UseSerilogRequestLogging();

// Set up default route for MVC controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure Razor Pages are mapped
app.MapRazorPages();

app.Run();
