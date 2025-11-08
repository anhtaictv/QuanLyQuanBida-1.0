using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyQuanBida.Application.Services
{
    public class ShiftService : IShiftService
    {
        private readonly QuanLyBidaDbContext _context;

        public ShiftService(QuanLyBidaDbContext context)
        {
            _context = context;
        }

        public async Task<Shift?> OpenShiftAsync(int userId, decimal openingCash)
        {
            // Check if user already has an open shift
            var existingShift = await _context.Shifts
                .Where(s => s.UserId == userId && s.EndAt == null)
                .FirstOrDefaultAsync();

            if (existingShift != null)
                return null; // User already has an open shift

            var newShift = new Shift
            {
                UserId = userId,
                StartAt = DateTime.UtcNow,
                OpeningCash = openingCash,
                ClosingCash = 0
            };

            _context.Shifts.Add(newShift);
            await _context.SaveChangesAsync();

            return newShift;
        }

        public async Task<bool> CloseShiftAsync(int shiftId, decimal closingCash, string notes)
        {
            var shift = await _context.Shifts.FindAsync(shiftId);
            if (shift == null || shift.EndAt != null)
                return false;

            shift.EndAt = DateTime.UtcNow;
            shift.ClosingCash = closingCash;
            shift.Notes = notes;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Shift?> GetCurrentShiftAsync(int userId)
        {
            return await _context.Shifts
                .Where(s => s.UserId == userId && s.EndAt == null)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Shift>> GetShiftHistoryAsync(int userId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Shifts
                .Where(s => s.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(s => s.StartAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.StartAt <= endDate.Value);

            return await query
                .OrderByDescending(s => s.StartAt)
                .ToListAsync();
        }
    }
}