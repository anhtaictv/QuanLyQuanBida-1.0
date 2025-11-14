using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using System; // <-- Thêm
using System.Collections.Generic; // <-- Thêm
using System.Linq; // <-- Thêm
using System.Threading.Tasks; // <-- Thêm

namespace QuanLyQuanBida.Application.Services
{
    public class AuditService : IAuditService
    {
        // SỬA LỖI: Dùng DbContextFactory thay vì DbContext trực tiếp
        // Điều này ngăn lỗi "Cannot consume scoped service from singleton"
        private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;

        public AuditService(IDbContextFactory<QuanLyBidaDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task LogActionAsync(int userId, string action, string? targetTable = null, int? targetId = null, string? oldValue = null, string? newValue = null)
        {
            // Tạo một DbContext mới chỉ dùng cho hành động này
            await using var context = await _contextFactory.CreateDbContextAsync();

            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                TargetTable = targetTable,
                TargetId = targetId,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedAt = DateTime.UtcNow // <-- HOÀN THIỆN: Gán thời gian
            };

            context.AuditLogs.Add(auditLog);
            await context.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? startDate, DateTime? endDate, string? action = null, int? userId = null)
        {
            // Tạo một DbContext mới chỉ dùng cho hành động này
            await using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
            {
                // Lấy từ 00:00:00 của ngày bắt đầu
                query = query.Where(l => l.CreatedAt >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                // Lấy đến 23:59:59 của ngày kết thúc
                query = query.Where(l => l.CreatedAt <= endDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            if (!string.IsNullOrEmpty(action))
                query = query.Where(l => l.Action.Contains(action));

            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId.Value);

            return await query
                .Include(l => l.User) // Giữ nguyên Include để lấy tên User
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }
    }
}