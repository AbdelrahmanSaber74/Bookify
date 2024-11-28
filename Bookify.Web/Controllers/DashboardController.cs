﻿using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Bookify.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {

        private readonly IBookCopyRepo _copyRepo;
        private readonly ISubscribersRepo _subscribersRepo;
        private readonly IBookRepo _book;
        private readonly IRentalCopyRepo _rentalCopy;
        private readonly IMapper _mapper;

        public DashboardController(IBookCopyRepo copyRepo, ISubscribersRepo subscribersRepo, IMapper mapper, IBookRepo book, IRentalCopyRepo rentalCopy)
        {
            _copyRepo = copyRepo;
            _subscribersRepo = subscribersRepo;
            _mapper = mapper;
            _book = book;
            _rentalCopy = rentalCopy;
        }

        public async Task<IActionResult> Index()
        {
            var numberOfCopies = await _copyRepo.Count();
            var numberOfSubscribers = await _subscribersRepo.Count();
            var lastAddedBooks = await _book.LastAddedBooks();
            var topBooks = await _book.TopBooks();

            var model = new DashboardViewModel
            {
                NumberOfCopies = numberOfCopies,
                NumberOfSubscribers = numberOfSubscribers,
                LastAddedBooks = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks),
                TopBooks = _mapper.Map<IEnumerable<BookViewModel>>(topBooks)
            };


            return View(model);

        }

        public async Task<IActionResult> GetRentalsPerDay(DateTime? startDate, DateTime? endDate)
        {
            var data = await _rentalCopy.GetRentalsPerDayAsync(startDate, endDate);
            return Ok(data);
        }

		public async Task<IActionResult> GetSubscribersPerCity()
		{
            var data = await _subscribersRepo.GetSubscribersPerCity();
			return Ok(data);

		}




	}
}
