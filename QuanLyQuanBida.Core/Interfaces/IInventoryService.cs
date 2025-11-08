using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IInventoryService
    {
        Task<List<InventoryTransaction>> GetInventoryTransactionsAsync(int productId);
        Task<bool> AddStockAsync(int productId, decimal quantity, string reference, int userId);
        Task<bool> RemoveStockAsync(int productId, decimal quantity, string reference, int userId);
        Task<List<Product>> GetLowStockProductsAsync();
    }
}