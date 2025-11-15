using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface ISessionService
    {
        // Đã sửa: Thêm rateId và trả về Session
        Task<Session?> StartSessionAsync(int tableId, int userId, int rateId);

        Task<bool> PauseSessionAsync(int sessionId, int userId);
        Task<bool> ResumeSessionAsync(int sessionId, int userId);
        Task<Session?> CloseSessionAsync(int sessionId, int userId);
        Task<bool> MoveSessionAsync(int sessionId, int targetTableId, int userId);
        Task<Session?> GetActiveSessionByTableIdAsync(int tableId);
        Task<bool> AssignCustomerToSessionAsync(int sessionId, int? customerId);
    }
}