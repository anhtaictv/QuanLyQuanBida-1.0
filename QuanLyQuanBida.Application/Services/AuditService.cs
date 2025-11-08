using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly QuanLyBidaDbContext _context;

        public AuditService(QuanLyBidaDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(int userId, string action, string? targetTable = null, int? targetId = null, string? oldValue = null, string? newValue = null)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                TargetTable = targetTable,
                TargetId = targetId,
                OldValue = oldValue,
                NewValue = newValue
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? startDate, DateTime? endDate, string? action = null, int? userId = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(l => l.Action.Contains(action));

            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId.Value);

            return await query
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }
    }
}