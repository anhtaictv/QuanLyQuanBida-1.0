using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly QuanLyBidaDbContext _context;

        public OrderService(QuanLyBidaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetOrdersBySessionIdAsync(int sessionId)
        {
            return await _context.Orders
                .Include(o => o.Product)
                .Where(o => o.SessionId == sessionId)
                .ToListAsync();
        }

        public async Task<Order?> CreateOrderAsync(OrderDto orderDto)
        {
            var product = await _context.Products.FindAsync(orderDto.ProductId);
            if (product == null)
            {
                return null;
            }

            var newOrder = new Order
            {
                SessionId = orderDto.SessionId,
                ProductId = orderDto.ProductId,
                Quantity = orderDto.Quantity,
                Price = orderDto.Price,
                Note = orderDto.Note,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(newOrder);

            // Update inventory if tracked
            if (product.IsInventoryTracked)
            {
                // We need to add inventory tracking logic
            }

            await _context.SaveChangesAsync();

            return newOrder;
        }

        public async Task<bool> UpdateOrderAsync(OrderDto orderDto)
        {
            var order = await _context.Orders.FindAsync(orderDto.Id);
            if (order == null)
            {
                return false;
            }

            order.Quantity = orderDto.Quantity;
            order.Price = orderDto.Price;
            order.Note = orderDto.Note;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}