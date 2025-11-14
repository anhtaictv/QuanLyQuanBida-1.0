using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;
        private readonly IInventoryService _inventoryService;

        public OrderService(
            IDbContextFactory<QuanLyBidaDbContext> contextFactory, 
            IInventoryService inventoryService)
        {
            _contextFactory = contextFactory;
            _inventoryService = inventoryService;
        }

        public async Task<List<Order>> GetOrdersBySessionIdAsync(int sessionId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Orders
                .Include(o => o.Product)
                .Where(o => o.SessionId == sessionId)
                .ToListAsync();
        }

        public async Task<Order?> CreateOrderAsync(OrderDto orderDto)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var product = await context.Products.FindAsync(orderDto.ProductId);
            if (product == null) return null;
            var newOrder = new Order
            {
                SessionId = orderDto.SessionId,
                ProductId = orderDto.ProductId,
                Quantity = orderDto.Quantity,
                Price = product.Price,
                Note = orderDto.Note,
                CreatedAt = DateTime.UtcNow
            };
            context.Orders.Add(newOrder);
            await context.SaveChangesAsync(); 

            if (product.IsInventoryTracked)
            {
                await _inventoryService.DeductStockAsync(product.Id, newOrder.Quantity, newOrder.Id);
            }

            return newOrder;
        }

        public async Task<bool> UpdateOrderAsync(OrderDto orderDto)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var order = await context.Orders.FindAsync(orderDto.Id);
            if (order == null)
            {
                return false;
            }

            order.Quantity = orderDto.Quantity;
            order.Price = orderDto.Price;
            order.Note = orderDto.Note;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            var order = await context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            context.Orders.Remove(order);
            await context.SaveChangesAsync();
            return true;
        }
    }
}