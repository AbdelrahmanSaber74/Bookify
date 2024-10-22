using Bookify.Web.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Data;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Bookify.Web.Controllers
{
	[Authorize(Roles = AppRoles.Admin)]
	public class UsersController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IMapper _mapper;
		private readonly IEmailSender _emailSender;
		private readonly ILogger<UsersController> _logger;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly string _resetPassword;
		private readonly IEmailBodyBuilder _emailBodyBuilder;

		public UsersController(IEmailBodyBuilder emailBodyBuilder, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IMapper mapper, IConfiguration configuration, IEmailSender emailSender, ILogger<UsersController> logger, IWebHostEnvironment webHostEnvironment)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
			_mapper = mapper;
			_emailSender = emailSender;
			_logger = logger;
			_webHostEnvironment = webHostEnvironment;
			_emailBodyBuilder = emailBodyBuilder;
			_resetPassword = configuration["ResetPassword"]!;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var users = await _userManager.Users.ToListAsync();
			var viewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);
			return View(viewModel);
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			return await ReturnFormViewWithError(null);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddUser(AddEditUserViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return await ReturnFormViewWithError(model);
			}

			var newUser = CreateUser(model);
			var result = await _userManager.CreateAsync(newUser, model.Password!);

			if (result.Succeeded)
			{
				await SendConfirmationEmail(newUser, model.FullName, model.Email);
				await _userManager.AddToRolesAsync(newUser, model.SelectedRoles);
				TempData["SuccessMessage"] = "User added successfully!";
				return RedirectToAction(nameof(Index));
			}

			AddErrorsToModelState(result.Errors);
			return await ReturnFormViewWithError(model);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null) return NotFound();

			var userRoles = await _userManager.GetRolesAsync(user);
			var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
			var roleItems = allRoles.Select(role => new SelectListItem
			{
				Value = role,
				Text = role,
			}).ToList();

			var viewModel = new AddEditUserViewModel
			{
				Id = user.Id,
				FullName = user.FullName,
				UserName = user.UserName,
				PhoneNumber = user.PhoneNumber,
				Email = user.Email,
				Roles = roleItems,
				SelectedRoles = userRoles.ToList()
			};

			return View("Form", viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditUser(AddEditUserViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return await ReturnFormViewWithError(model);
			}

			var user = await _userManager.FindByIdAsync(model.Id!);
			if (user == null)
			{
				TempData["ErrorMessage"] = "User not found.";
				return RedirectToAction(nameof(Index));
			}

			_mapper.Map(model, user);
			user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			user.LastUpdatedOn = DateTime.Now;

			var updateUserResult = await _userManager.UpdateAsync(user);
			if (!updateUserResult.Succeeded)
			{
				TempData["WarningMessage"] = "Failed to update user details.";
				return RedirectToAction(nameof(Index));
			}

			await UpdateUserRoles(user, model.SelectedRoles);
			await _userManager.UpdateSecurityStampAsync(user);

			TempData["SuccessMessage"] = "User updated successfully!";
			return RedirectToAction(nameof(Index));
		}

		[AllowAnonymous]
		public async Task<IActionResult> LogOut()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ToggleStatus(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null) return NotFound();

			user.IsDeleted = !user.IsDeleted;
			user.LastUpdatedOn = DateTime.Now;
			user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			await _userManager.UpdateAsync(user);
			if (user.IsDeleted) await _userManager.UpdateSecurityStampAsync(user);

			return Json(new { success = true, lastUpdatedOn = user.LastUpdatedOn.ToString() });
		}

		[HttpGet]
		public async Task<IActionResult> ResetPassword(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null) return NotFound();

			var currentPassword = user.PasswordHash;
			var removeResult = await _userManager.RemovePasswordAsync(user);
			if (!removeResult.Succeeded)
			{
				TempData["WarningMessage"] = "Unable to remove the current password.";
				return RedirectToAction(nameof(Index));
			}

			var result = await _userManager.AddPasswordAsync(user, _resetPassword);
			if (result.Succeeded)
			{
				user.LastUpdatedOn = DateTime.Now;
				user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				await _userManager.UpdateAsync(user);
				TempData["SuccessMessage"] = "Password has been reset successfully!";
				return RedirectToAction(nameof(Index));
			}

			await _userManager.AddPasswordAsync(user, currentPassword!);
			await _userManager.UpdateAsync(user);

			var errorMessages = result.Errors.Select(e => e.Description).ToArray();
			TempData["WarningMessage"] = "Password was not reset successfully! " + string.Join(", ", errorMessages);
			return RedirectToAction(nameof(Index));
		}

		[AcceptVerbs("Get", "Post")]
		public async Task<IActionResult> AllowUserName(string userName, string id)
		{
			var user = await _userManager.FindByNameAsync(userName);
			if (user != null && user.Id != id)
			{
				return Json(string.Format(Errors.Duplicated, "User"));
			}

			return Json(true);
		}

		[HttpGet]
		public async Task<IActionResult> AllowEmail(string id, string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user != null && user.Id != id)
			{
				return Json(string.Format(Errors.Duplicated, "User"));
			}

			return Json(true);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Unlock(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null) return NotFound();

			if (await _userManager.IsLockedOutAsync(user))
			{
				await _userManager.SetLockoutEndDateAsync(user, null);
			}

			return Ok();
		}

		private async Task<IActionResult> ReturnFormViewWithError(AddEditUserViewModel? model)
		{
			model ??= new AddEditUserViewModel();

			var roles = await _roleManager.Roles
				.Select(r => new SelectListItem
				{
					Value = r.Name,
					Text = r.Name
				})
				.ToListAsync();

			model.Roles = roles;
			return View("Form", model);
		}

		private ApplicationUser CreateUser(AddEditUserViewModel model)
		{
			return new ApplicationUser
			{
				UserName = model.FullName,
				NormalizedUserName = model.Email.ToUpper(),
				FullName = model.FullName,
				Email = model.Email,
				NormalizedEmail = model.Email.ToUpper(),
				CreatedOn = DateTime.Now,
				CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
				PhoneNumber = model.PhoneNumber,
			};
		}

		private async Task SendConfirmationEmail(ApplicationUser newUser, string fullName, string email)
		{

			// Generate the email confirmation token and callback URL
			var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

			var callbackUrl = Url.Page(
					"/Account/ConfirmEmail",
					pageHandler: null,
					values: new { area = "Identity", userId = newUser.Id, code },
					protocol: Request.Scheme);


			var body = await _emailBodyBuilder.GetEmailBodyAsync(
							"https://res.cloudinary.com/dkbsaseyc/image/upload/fl_preserve_transparency/v1729535424/icon-positive-vote-1_rdexez_ii8um2.jpg?_s=public-apps",
							$"Hey {fullName}",
							"Please Confirm your account",
							"Active Account",
							callbackUrl!
						);


			await _emailSender.SendEmailAsync(email, "Confirm your email", body);

		}

		private void AddErrorsToModelState(IEnumerable<IdentityError> errors)
		{
			foreach (var error in errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		private async Task UpdateUserRoles(ApplicationUser user, IList<string> selectedRoles)
		{
			var currentRoles = await _userManager.GetRolesAsync(user);
			var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
			var rolesToRemove = currentRoles.Except(selectedRoles).ToList();

			await _userManager.AddToRolesAsync(user, rolesToAdd);
			await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
		}
	}
}
