using Bookify.Web.Repositories.Authors;

namespace Bookify.Web.Controllers
{
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
            var authorDtos = _mapper.Map<IEnumerable<AuthorDTO>>(authors);
            return View(authorDtos);
        }

        // GET: Authors/Create
        public IActionResult Create()
        {
            return View("AddAuthor");
        }

        // POST: Authors/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAuthor(AuthorDTO authorDTO)
        {
            if (!ModelState.IsValid)
            {
                return View("AddAuthor", authorDTO);
            }

            var newAuthor = _mapper.Map<Author>(authorDTO);
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

            var authorDTO = _mapper.Map<AuthorDTO>(author);
            return View("EditAuthor", authorDTO);
        }

        // POST: Authors/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAuthor(AuthorDTO authorDTO)
        {
            if (!ModelState.IsValid)
            {
                return View("EditAuthor", authorDTO);
            }

            var existingAuthor = await _authorRepo.GetAuthorByIdAsync(authorDTO.Id);
            if (existingAuthor == null)
            {
                return NotFound();
            }

            _mapper.Map(authorDTO, existingAuthor);
            existingAuthor.LastUpdatedOn = DateTime.Now;

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
                return Json($"The author name '{name}' is already taken.");
            }

            return Json(true);
        }
    }
}
