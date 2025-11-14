using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IInventoryService
    {
        // Trừ tồn kho khi tạo Order (bán hàng)
        Task<bool> DeductStockAsync(int productId, int quantity, int orderId);

        // Cập nhật tồn kho (Nhập/Xuất/Kiểm kê)
        Task<bool> UpdateStockAsync(int productId, int quantity, string transactionType, string reference, int userId);

        // Lấy số lượng tồn kho hiện tại (cần tính toán)
        Task<int> GetCurrentStockAsync(int productId);
    }
}