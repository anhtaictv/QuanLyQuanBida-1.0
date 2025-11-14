using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class RateService : IRateService
    {

        private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;

        public RateService(IDbContextFactory<QuanLyBidaDbContext> contextFactory) 
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Rate>> GetAllRatesAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            return await context.Rates.ToListAsync();
        }

        public async Task<Rate?> GetRateByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            return await context.Rates.FindAsync(id);
        }

        public async Task<Rate?> GetApplicableRateAsync(DateTime dateTime)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var time = TimeOnly.FromDateTime(dateTime);
            var isWeekend = dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
            return await context.Rates
                .Where(r =>
                    (!r.StartTimeWindow.HasValue || r.StartTimeWindow <= time) && 
                    (!r.EndTimeWindow.HasValue || r.EndTimeWindow >= time) && 
                    (r.IsWeekendRate == isWeekend || !r.IsWeekendRate)) 
                .OrderByDescending(r => r.IsDefault) 
                .FirstOrDefaultAsync();
        }

        public async Task<Rate> CreateRateAsync(Rate rate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Rates.Add(rate);
            await context.SaveChangesAsync();
            return rate;
        }

        public async Task<bool> UpdateRateAsync(Rate rate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Rates.Update(rate);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRateAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            var rate = await context.Rates.FindAsync(id);
            if (rate == null)
                return false;
            context.Rates.Remove(rate);
            await context.SaveChangesAsync();
            return true;
        }
    }
}