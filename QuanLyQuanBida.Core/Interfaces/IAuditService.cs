using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(int userId, string action, string? targetTable = null, int? targetId = null, string? oldValue = null, string? newValue = null);
        Task<List<AuditLog>> GetAuditLogsAsync(DateTime? startDate, DateTime? endDate, string? action = null, int? userId = null);
    }
}