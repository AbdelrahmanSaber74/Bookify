// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Bookify.Web.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bookify.Web.Areas.Identity.Pages.Account.Manage
{
	public class IndexModel : PageModel
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IImageService _imageService;

		public IndexModel(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			IImageService imageService)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_imageService = imageService;
		}

		[TempData]
		public string StatusMessage { get; set; }

		public string Username { get; set; }

		[BindProperty]
		public InputModel Input { get; set; }

		public class InputModel
		{
			[Required(ErrorMessage = "Full Name is required.")]
			[MaxLength(100, ErrorMessage = Errors.MaxLength)]
			[Display(Name = "Full Name")]
			[RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
			public string FullName { get; set; } = null!;

			[Phone]
			[Display(Name = "Phone number")]
			[MaxLength(11, ErrorMessage = Errors.MaxLength)]
			[RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
			public string PhoneNumber { get; set; }

			public IFormFile Avatar { get; set; }

			public bool ImageRemoved { get; set; }
		}

		private async Task LoadAsync(ApplicationUser user)
		{
			Username = await _userManager.GetUserNameAsync(user);
			Input = new InputModel
			{
				FullName = user.FullName,
				PhoneNumber = await _userManager.GetPhoneNumberAsync(user)
			};
		}

		public async Task<IActionResult> OnGetAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			await LoadAsync(user);
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			if (!ModelState.IsValid)
			{
				await LoadAsync(user);
				return Page();
			}

			// Handle Avatar upload
			if (Input.Avatar != null)
			{
				// Delete old image
				_imageService.DeleteOldImages(user.ImageUrl, null);

				// Save new image
				var result = await _imageService.SaveImageAsync(Input.Avatar, "images/users", false);

				if (result is OkObjectResult okResult)
				{
					// Extract the result data
					var resultData = okResult.Value as dynamic;

					// Make sure resultData contains the expected properties
					if (resultData != null)
					{
						user.ImageUrl = resultData.relativePath; // Assuming these are the keys you used in SaveImageAsync

						// Update the user in the database
						await _userManager.UpdateAsync(user);
					}
				}

				else if (result is BadRequestObjectResult badRequestResult)
				{
					var errorMessage = badRequestResult.Value?.ToString() ?? Errors.ImageSaveFailed;
					ModelState.AddModelError(string.Empty, errorMessage);
					return Page();
				}



			}
			else if (Input.ImageRemoved)
			{
				_imageService.DeleteOldImages(user.ImageUrl, null);
				user.ImageUrl = null;
				await _userManager.UpdateAsync(user);
			}
			// Update Phone Number
			if (Input.PhoneNumber != await _userManager.GetPhoneNumberAsync(user))
			{
				var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
				if (!setPhoneResult.Succeeded)
				{
					StatusMessage = "Unexpected error when trying to set phone number.";
					return RedirectToPage();
				}
			}

			// Update Full Name
			if (Input.FullName != user.FullName)
			{
				user.FullName = Input.FullName;
				var setFullNameResult = await _userManager.UpdateAsync(user);
				if (!setFullNameResult.Succeeded)
				{
					StatusMessage = "Unexpected error when trying to set full name.";
					return RedirectToPage();
				}
			}

			await _signInManager.RefreshSignInAsync(user);
			StatusMessage = "Your profile has been updated";
			return RedirectToPage();
		}


	}
}
