using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;
        private readonly ICurrentUserService _currentUserService;

        public InventoryService(
            IDbContextFactory<QuanLyBidaDbContext> contextFactory, 
            ICurrentUserService currentUserService) 
        {
            _contextFactory = contextFactory; 
            _currentUserService = currentUserService; 
        }

        public async Task<bool> DeductStockAsync(int productId, int quantity, int orderId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            var product = await context.Products.FindAsync(productId);
            if (product == null || !product.IsInventoryTracked)
                return true;
            // 1. Ghi nhận giao dịch xuất kho
            var transaction = new InventoryTransaction
            {
                ProductId = productId,
                Type = "OUT",
                Quantity = quantity,
                Reference = $"Order #{orderId}",
                CreatedBy = _currentUserService.CurrentUser?.Id ?? 0,
                CreatedAt = DateTime.UtcNow
            };
        context.InventoryTransactions.Add(transaction);
            // 2. Cập nhật tồn kho trong bảng Products
            product.Quantity -= quantity;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCurrentStockAsync(int productId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            var product = await context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);
            return product?.Quantity ?? 0;
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity, string transactionType, string reference, int userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var transaction = new InventoryTransaction
            {
                ProductId = productId,
                Type = transactionType,
                Quantity = quantity,
                Reference = reference,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };
            context.InventoryTransactions.Add(transaction);
            var product = await context.Products.FindAsync(productId);
            if (product == null) return false;
            if (transactionType == "IN")
                product.Quantity += quantity;
            else if (transactionType == "OUT")
                product.Quantity -= quantity;
            else if (transactionType == "ADJUST")
                product.Quantity = quantity;
            await context.SaveChangesAsync();
            return true;
        }
    }
}