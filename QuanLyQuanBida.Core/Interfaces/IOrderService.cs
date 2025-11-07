using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.DTOs;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersBySessionIdAsync(int sessionId);
        Task<Order?> CreateOrderAsync(OrderDto orderDto);
        Task<bool> UpdateOrderAsync(OrderDto orderDto);
        Task<bool> DeleteOrderAsync(int orderId);
    }
}