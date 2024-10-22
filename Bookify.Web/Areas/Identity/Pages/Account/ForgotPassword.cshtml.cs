// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Bookify.Web.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace Bookify.Web.Areas.Identity.Pages.Account
{
	public class ForgotPasswordModel : PageModel
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IEmailSender _emailSender;
		private readonly IEmailBodyBuilder _emailBodyBuilder;

		public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
		{
			_userManager = userManager;
			_emailSender = emailSender;
			_emailBodyBuilder = emailBodyBuilder;
		}

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[BindProperty]
		public InputModel Input { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public class InputModel
		{
			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			[EmailAddress]
			public string Email { get; set; }
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(Input.Email);
				if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
				{
					// Don't reveal that the user does not exist or is not confirmed
					return RedirectToPage("./ForgotPasswordConfirmation");
				}

				var code = await _userManager.GeneratePasswordResetTokenAsync(user);

				code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
				var callbackUrl = Url.Page(
					"/Account/ResetPassword",
					pageHandler: null,
					values: new { area = "Identity", code },
					protocol: Request.Scheme);




				var body = await _emailBodyBuilder.GetEmailBodyAsync(
								"https://res.cloudinary.com/dkbsaseyc/image/upload/fl_preserve_transparency/v1729557070/icon-positive-vote-2_jcxdww_rghb1a.jpg?_s=public-apps",
								$"Hello {user.FullName},",
								"We received a request to reset your password. If you didn't make this request, you can ignore this email.",
								"Reset Your Password",
								callbackUrl!
							);

				await _emailSender.SendEmailAsync(user.Email, "Confirm your email", body);
			
				return RedirectToPage("./ForgotPasswordConfirmation");
			}

			return Page();
		}
	}
}
