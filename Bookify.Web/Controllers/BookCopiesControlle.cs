using Bookify.Web.Repositories.BookCopies;

namespace Bookify.Web.Controllers
{
    public class BookCopiesController : Controller
    {
        private readonly IBookCopyRepo _copyRepo;
        private readonly IMapper _mapper;

        public BookCopiesController(IBookCopyRepo copyRepo, IMapper mapper) // Change Mapper to IMapper
        {
            _copyRepo = copyRepo;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var bookCopy = await _copyRepo.GetBookCopyByIdAsync(id);

            if (bookCopy == null)
            {
                return NotFound();
            }

            // Toggle the IsDelete property
            bookCopy.IsDeleted = !bookCopy.IsDeleted;
            bookCopy.LastUpdatedOn = DateTime.Now;

            // Update the book copy in the repository
            await _copyRepo.UpdateBookCopyAsync(bookCopy);

            return Json(new
            {
                success = true,
                lastUpdatedOn = bookCopy.LastUpdatedOn.ToString()
            });
        }
    }
}
