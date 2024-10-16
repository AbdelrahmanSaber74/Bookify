using Bookify.Web.Core.Consts;
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


        public UsersController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _mapper = mapper;

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

            var result = await _userManager.CreateAsync(newUser, model.Password);

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


        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> AllowEmail(string Email, string Id)
        {
            var user = await _userManager.FindByEmailAsync(Email);

            if (user != null && user.Id != Id)
            {
                var errorMessage = string.Format(Errors.Duplicated, "User");

                return Json(string.Format(errorMessage));

            }


            return Json(true);

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
