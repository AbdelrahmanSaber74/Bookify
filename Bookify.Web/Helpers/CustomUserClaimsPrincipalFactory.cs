using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
	public CustomUserClaimsPrincipalFactory(
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole> roleManager,
		IOptions<IdentityOptions> optionsAccessor)
		: base(userManager, roleManager, optionsAccessor)
	{
	}

	protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
	{
		// Get the default claims identity
		var identity = await base.GenerateClaimsAsync(user);

		// Add custom FullName claim if it doesn't already exist
		var fullNameClaim = identity.FindFirst(ClaimTypes.GivenName);
		if (fullNameClaim == null)
		{
			// Assuming ApplicationUser has FirstName and LastName properties
			var fullName = $"{user.FullName}".Trim();
			identity.AddClaim(new Claim(ClaimTypes.GivenName, fullName));
		}

		// You can add other custom claims here as well, like user roles or permissions

		return identity;
	}
}
