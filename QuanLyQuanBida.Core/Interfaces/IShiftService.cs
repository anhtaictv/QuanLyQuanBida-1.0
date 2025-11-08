using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IShiftService
    {
        Task<Shift?> OpenShiftAsync(int userId, decimal openingCash);
        Task<bool> CloseShiftAsync(int shiftId, decimal closingCash, string notes);
        Task<Shift?> GetCurrentShiftAsync(int userId);
        Task<List<Shift>> GetShiftHistoryAsync(int userId, DateTime? startDate, DateTime? endDate);
    }
}