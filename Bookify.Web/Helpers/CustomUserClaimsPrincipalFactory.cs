using Microsoft.Extensions.Options;
using System.Security.Claims;

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
			// Assuming ApplicationUser has FullName property
			var fullName = $"{user.FullName}".Trim();
			identity.AddClaim(new Claim(ClaimTypes.GivenName, fullName));
		}

		// Add custom ImageUrl claim if it doesn't already exist
		var imageUrlClaim = identity.FindFirst("ImageUrl");
		if (imageUrlClaim == null && !string.IsNullOrEmpty(user.ImageUrl))
		{
			identity.AddClaim(new Claim("ImageUrl", user.ImageUrl));
		}

		// Add custom ThumbnailUrl claim if it doesn't already exist
		var thumbnailUrlClaim = identity.FindFirst("ThumbnailUrl");
		if (thumbnailUrlClaim == null && !string.IsNullOrEmpty(user.ThumbnailUrl))
		{
			identity.AddClaim(new Claim("ThumbnailUrl", user.ThumbnailUrl));
		}

		return identity;
	}
}
