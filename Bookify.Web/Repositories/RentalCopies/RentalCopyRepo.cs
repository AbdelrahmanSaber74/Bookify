﻿namespace Bookify.Web.Repositories.RentalCopies
{
    public class RentalCopyRepo : IRentalCopyRepo
    {
        private readonly ApplicationDbContext _context;

        public RentalCopyRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RentalCopy> GetByIdAsync(int id)
        {
            return await _context.RentalCopies.FindAsync(id);
        }

        public async Task<IEnumerable<RentalCopy>> GetAllAsync()
        {
            return await _context.RentalCopies.ToListAsync();
        }

        public async Task AddAsync(RentalCopy rentalCopy)
        {
            await _context.RentalCopies.AddAsync(rentalCopy);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RentalCopy rentalCopy)
        {
            _context.RentalCopies.Update(rentalCopy);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rentalCopy = await _context.RentalCopies.FindAsync(id);
            if (rentalCopy != null)
            {
                _context.RentalCopies.Remove(rentalCopy);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> AnyAsync(Expression<Func<RentalCopy, bool>> predicate)
        {
            return await _context.RentalCopies.AnyAsync(predicate);
        }

        public async Task<int> CountRentalCopiesByIdAsync(int id)
        {
            return await _context.RentalCopies.CountAsync(r => r.RentalId == id);
        }

        public async Task<IEnumerable<RentalCopy>> GetRentalHistoryAsync(int id)
        {
            return await _context.RentalCopies
                .Include(r => r.Rental)
                .ThenInclude(r => r!.Subscriber)
                .Where(r => r.BookCopyId == id)
                .OrderByDescending(r => r.RentalDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<RentalCopy>> GetAllByRentalIdAsync(int rentalId)
        {
            return await _context.RentalCopies
                .Where(r => r.RentalId == rentalId)
                .ToListAsync();
        }

        public async Task<RentalCopy> GetByBookCopyIdAsync(int rentalId, int bookCopId)
        {
            return await _context.RentalCopies
                            .Where(r => r.RentalId == rentalId && r.BookCopyId == bookCopId)
                            .FirstOrDefaultAsync();
        }
    }
}
