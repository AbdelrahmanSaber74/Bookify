using System.Security.Claims;

namespace Bookify.Web.Controllers
{
	[Authorize(Roles = AppRoles.Archive)]
	public class AuthorsController : Controller
	{
		private readonly IAuthorRepo _authorRepo;
		private readonly IMapper _mapper;

		public AuthorsController(IAuthorRepo authorRepo, IMapper mapper)
		{
			_authorRepo = authorRepo;
			_mapper = mapper;
		}

		// GET: Authors
		public async Task<IActionResult> Index()
		{
			var authors = await _authorRepo.GetAllAuthorsAsync();
			var authorViewModels = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);
			return View(authorViewModels);
		}

		// GET: Authors/Create
		public IActionResult Create()
		{
			return View("AddAuthor");
		}

		// POST: Authors/Add
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddAuthor(AuthorViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View("AddAuthor", model);
			}

			var newAuthor = _mapper.Map<Author>(model);
			newAuthor.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			await _authorRepo.AddAuthorAsync(newAuthor);

			TempData["SuccessMessage"] = "Author added successfully!";
			return RedirectToAction(nameof(Index));
		}

		// POST: Authors/Delete/{id}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			var existingAuthor = await _authorRepo.GetAuthorByIdAsync(id);
			if (existingAuthor == null)
			{
				return Json(new { success = false, message = "Author not found." });
			}

			await _authorRepo.DeleteAuthorAsync(id);
			return Json(new { success = true });
		}

		// POST: Authors/ToggleStatus/{id}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ToggleStatus(int id)
		{
			var existingAuthor = await _authorRepo.GetAuthorByIdAsync(id);
			if (existingAuthor == null)
			{
				return NotFound();
			}

			existingAuthor.IsDeleted = !existingAuthor.IsDeleted;
			existingAuthor.LastUpdatedOn = DateTime.Now;
			existingAuthor.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			await _authorRepo.UpdateAuthorAsync(existingAuthor);

			return Json(new
			{
				success = true,
				lastUpdatedOn = existingAuthor.LastUpdatedOn.ToString()
			});
		}

		// GET: Authors/Edit/{id}
		public async Task<IActionResult> Edit(int id)
		{
			var author = await _authorRepo.GetAuthorByIdAsync(id);
			if (author == null)
			{
				return NotFound();
			}

			var authorViewModel = _mapper.Map<AuthorViewModel>(author);
			return View("EditAuthor", authorViewModel);
		}

		// POST: Authors/Update
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateAuthor(AuthorViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View("EditAuthor", model);
			}

			var existingAuthor = await _authorRepo.GetAuthorByIdAsync(model.Id);
			if (existingAuthor == null)
			{
				return NotFound();
			}

			_mapper.Map(model, existingAuthor);
			existingAuthor.LastUpdatedOn = DateTime.Now;
			existingAuthor.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			await _authorRepo.UpdateAuthorAsync(existingAuthor);

			TempData["SuccessMessage"] = "Author updated successfully!";
			return RedirectToAction(nameof(Index));
		}

		// Check if author name is unique
		[AcceptVerbs("Get", "Post")]
		public async Task<IActionResult> IsAuthorNameUnique(string name, int id)
		{
			var isNameTaken = await _authorRepo.AnyAsync(c => c.Name == name && c.Id != id);
			if (isNameTaken)
			{
				return Json(string.Format(Errors.Duplicated, name));
			}

			return Json(true);
		}
	}
}
