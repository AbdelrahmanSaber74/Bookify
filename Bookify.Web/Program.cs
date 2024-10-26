var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
	?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));

// Use AddIdentity to register both UserManager and RoleManager
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	options.SignIn.RequireConfirmedAccount = true;



	options.Password.RequiredLength = 8; // Minimum length of the password

	options.User.RequireUniqueEmail = true;



})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();



// Register all additional services
builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();

// Create a service scope to resolve UserManager and RoleManager for seeding
using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

	// Seed roles and admin user
	await SeedRoles.AddDefaultRolesAsync(roleManager);
	await DefaultUser.SeedAdminUser(userManager);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure Razor Pages are mapped
app.MapRazorPages();

app.Run();
