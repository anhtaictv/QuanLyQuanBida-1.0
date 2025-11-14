using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.DTOs; // Cần tạo ShiftDto

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IShiftService
    {
        Task<Shift?> OpenShiftAsync(int userId, decimal openingCash);
        Task<Shift?> CloseShiftAsync(int shiftId, decimal closingCash, string notes);
        Task<Shift?> GetActiveShiftByUserIdAsync(int userId);
    }
}