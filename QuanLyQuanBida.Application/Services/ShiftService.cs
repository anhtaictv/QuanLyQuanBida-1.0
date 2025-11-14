using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using System;
using System.Threading.Tasks;

namespace QuanLyQuanBida.Application.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;

        public ShiftService(IDbContextFactory<QuanLyBidaDbContext> contextFactory) 
        {
            _contextFactory = contextFactory;
        }

        public async Task<Shift?> OpenShiftAsync(int userId, decimal openingCash)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            if (await GetActiveShiftByUserIdAsync(userId) != null) return null;
            var newShift = new Shift
            {
                UserId = userId,
                StartAt = DateTime.UtcNow,
                OpeningCash = openingCash,
                Notes = "Ca làm việc mới",
                Status = "OPEN"
            };
            context.Shifts.Add(newShift);
            await context.SaveChangesAsync();
            return newShift;
        }

        public async Task<Shift?> CloseShiftAsync(int shiftId, decimal closingCash, string notes)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var shift = await context.Shifts.FindAsync(shiftId);
            if (shift == null || shift.Status == "CLOSED") return null;
            shift.EndAt = DateTime.UtcNow;
            shift.ClosingCash = closingCash;
            shift.Notes = notes;
            shift.Status = "CLOSED";
            await context.SaveChangesAsync();
            return shift;
        }

        public async Task<Shift?> GetActiveShiftByUserIdAsync(int userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            return await context.Shifts
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "OPEN");
        }
    }
}