using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class RateService : IRateService
    {
        private readonly QuanLyBidaDbContext _context;

        public RateService(QuanLyBidaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Rate>> GetAllRatesAsync()
        {
            return await _context.Rates.ToListAsync();
        }

        public async Task<Rate?> GetRateByIdAsync(int id)
        {
            return await _context.Rates.FindAsync(id);
        }

        public async Task<Rate?> GetApplicableRateAsync(DateTime dateTime)
        {
            var time = TimeOnly.FromDateTime(dateTime);
            var isWeekend = dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;

            return await _context.Rates
                .Where(r =>
                    (r.StartTimeWindow <= time && r.EndTimeWindow >= time) &&
                    (r.IsWeekendRate == isWeekend || !r.IsWeekendRate))
                .FirstOrDefaultAsync();
        }

        public async Task<Rate> CreateRateAsync(Rate rate)
        {
            _context.Rates.Add(rate);
            await _context.SaveChangesAsync();
            return rate;
        }

        public async Task<bool> UpdateRateAsync(Rate rate)
        {
            _context.Rates.Update(rate);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRateAsync(int id)
        {
            var rate = await _context.Rates.FindAsync(id);
            if (rate == null)
                return false;

            _context.Rates.Remove(rate);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}