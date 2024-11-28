using Bookify.Infrastructure;
using Bookify.Web;
using Bookify.Web.Tasks;
using Hangfire.Dashboard;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddWebServices(builder);
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // From appsettings.json
    .CreateLogger();


var app = builder.Build();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using var scope = scopeFactory.CreateScope();

var roleManger = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManger = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

// Seed default roles and admin user
await SeedRoles.AddDefaultRolesAsync(roleManger);
await DefaultUser.SeedAdminUser(userManger);



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

//app.UseExceptionHandler(errorHandlingPath: "/Home/Error");
//app.UseStatusCodePagesWithReExecute("/Home/Error", "?StatusCode={0}");

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
