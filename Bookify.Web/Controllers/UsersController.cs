using System.Data;
using System.Security.Claims;

namespace Bookify.Web.Controllers
{

	[Authorize(Roles = AppRoles.Admin)]
	public class UsersController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ILogger<UsersController> _logger;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IMapper _mapper;
		private readonly string _resetPassword;
		private readonly IConfiguration _configuration;

		public UsersController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			RoleManager<IdentityRole> roleManager,
			IMapper mapper,
			IConfiguration configuration,
			ILogger<UsersController> logger)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
			_logger = logger;
			_mapper = mapper;
			_configuration = configuration;

			_resetPassword = _configuration["ResetPassword"];
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

			ApplicationUser newUser = new ApplicationUser
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

			var result = await _userManager.CreateAsync(newUser, model.Password!);

			if (result.Succeeded)
			{

				await _userManager.AddToRolesAsync(newUser, model.SelectedRoles);
				TempData["SuccessMessage"] = "User added successfully!";
				return RedirectToAction(nameof(Index));

			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return await ReturnFormViewWithError(model);



		}


		[HttpGet]
		public async Task<IActionResult> Edit(string Id)
		{
			var user = await _userManager.FindByIdAsync(Id);

			if (user == null)
			{
				return NotFound();
			}

			var userRoles = await _userManager.GetRolesAsync(user);

			var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

			var roleItems = allRoles.Select(role => new SelectListItem
			{
				Value = role,
				Text = role,
			}).ToList();

			var viewmodel = new AddEditUserViewModel
			{
				Id = user.Id,
				FullName = user.FullName,
				UserName = user.UserName,
				PhoneNumber = user.PhoneNumber,
				Email = user.Email,
				Roles = roleItems,
				SelectedRoles = userRoles.ToList()
			};


			return View("Form", viewmodel);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditUser(AddEditUserViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return await ReturnFormViewWithError(model);
			}

			// Find the user by ID
			var user = await _userManager.FindByIdAsync(model.Id!);
			if (user == null)
			{
				TempData["ErrorMessage"] = "User not found.";
				return RedirectToAction(nameof(Index));
			}

			// Update user information from model
			_mapper.Map(model, user);
			user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			user.LastUpdatedOn = DateTime.Now;

			// Update the user in the database
			var updateUserResult = await _userManager.UpdateAsync(user);
			if (!updateUserResult.Succeeded)
			{
				TempData["WarningMessage"] = "Failed to update user details.";
				return RedirectToAction(nameof(Index));
			}

			// Fetch the current roles and check if they need updating
			var currentRoles = await _userManager.GetRolesAsync(user);
			var rolesUpdated = !currentRoles.SequenceEqual(model.SelectedRoles);

			if (rolesUpdated)
			{
				// Remove old roles and add new roles
				var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
				if (!removeRolesResult.Succeeded)
				{
					var errorMessages = removeRolesResult.Errors.Select(e => e.Description).ToArray();
					TempData["WarningMessage"] = "Failed to remove existing roles: " + string.Join(", ", errorMessages);
					return RedirectToAction(nameof(Index));
				}

				var addRolesResult = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
				if (!addRolesResult.Succeeded)
				{
					var errorMessages = addRolesResult.Errors.Select(e => e.Description).ToArray();
					TempData["WarningMessage"] = "Failed to add new roles: " + string.Join(", ", errorMessages);
					return RedirectToAction(nameof(Index));
				}
			}

			// Success message
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
		public async Task<IActionResult> ToggleStatus(string Id)
		{
			var user = await _userManager.FindByIdAsync(Id);

			if (user == null)
			{
				return NotFound();
			}

			user.IsDeleted = !user.IsDeleted;
			user.LastUpdatedOn = DateTime.Now;
			user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			await _userManager.UpdateAsync(user);

			return Json(new
			{
				success = true,
				lastUpdatedOn = user.LastUpdatedOn.ToString()
			});

		}

		[HttpGet]
		public async Task<IActionResult> ResetPassword(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			var currentPassword = user.PasswordHash;

			// Remove the current password
			var removeResult = await _userManager.RemovePasswordAsync(user);
			if (!removeResult.Succeeded)
			{
				TempData["WarningMessage"] = "Unable to remove the current password.";
				return RedirectToAction(nameof(Index));
			}

			// Reset the password
			var result = await _userManager.AddPasswordAsync(user, _resetPassword);

			if (result.Succeeded)
			{
				user.LastUpdatedOn = DateTime.Now;
				user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

				// Update the user with the new password
				await _userManager.UpdateAsync(user);

				TempData["SuccessMessage"] = "Password has been reset successfully!";
				return RedirectToAction(nameof(Index));
			}

			// If the password reset fails, revert to the old password
			await _userManager.AddPasswordAsync(user, currentPassword);
			await _userManager.UpdateAsync(user);

			var errorMessages = result.Errors.Select(e => e.Description).ToArray();
			TempData["WarningMessage"] = "Password was not reset successfully! " + string.Join(", ", errorMessages);
			return RedirectToAction(nameof(Index));
		}



		[AcceptVerbs("Get", "Post")]
		public async Task<IActionResult> AllowUserName(string UserName, string Id)
		{

			var user = await _userManager.FindByNameAsync(UserName);

			if (user != null && user.Id != Id)
			{
				var errorMessage = string.Format(Errors.Duplicated, "User");

				return Json(string.Format(errorMessage));
			}

			return Json(true);

		}


		[HttpGet]
		public async Task<IActionResult> AllowEmail(string Id, string Email)
		{
			var user = await _userManager.FindByEmailAsync(Email);

			if (user != null && user.Id != Id)
			{
				var errorMessage = string.Format(Errors.Duplicated, "User");

				return Json(string.Format(errorMessage));

			}


			return Json(true);

		}


		[HttpPost]
		[ValidateAntiForgeryToken]

		public async Task<IActionResult> Unlock(string Id)
		{
			var user = await _userManager.FindByIdAsync(Id);

			if (user == null)
			{
				return NotFound();
			}



			var isLocked = await _userManager.IsLockedOutAsync(user);

			if (isLocked)
			{
				await _userManager.SetLockoutEndDateAsync(user, null);
			}

			return Ok();

		}


		private async Task<IActionResult> ReturnFormViewWithError(AddEditUserViewModel? model)
		{

			if (model == null)
			{
				model = new AddEditUserViewModel();
			}

			// Get the list of roles from the role manager
			var roles = await _roleManager.Roles
				.Select(r => new SelectListItem
				{
					Value = r.Name,
					Text = r.Name
				})
				.ToListAsync();

			// Populate the Roles property in the model
			model.Roles = roles;

			// Return the view with the model that includes the error
			return View("Form", model);
		}




	}
}
