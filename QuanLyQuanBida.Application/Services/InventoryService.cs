using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly QuanLyBidaDbContext _context;

        public InventoryService(QuanLyBidaDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryTransaction>> GetInventoryTransactionsAsync(int productId)
        {
            return await _context.InventoryTransactions
                .Where(t => t.ProductId == productId)
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> AddStockAsync(int productId, decimal quantity, string reference, int userId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return false;

            product.Quantity += (int)quantity;

            var transaction = new InventoryTransaction
            {
                ProductId = productId,
                Type = "IN",
                Quantity = quantity,
                Reference = reference,
                CreatedBy = userId
            };

            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveStockAsync(int productId, decimal quantity, string reference, int userId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Quantity < quantity)
                return false;

            product.Quantity -= (int)quantity;

            var transaction = new InventoryTransaction
            {
                ProductId = productId,
                Type = "OUT",
                Quantity = quantity,
                Reference = reference,
                CreatedBy = userId
            };

            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Product>> GetLowStockProductsAsync()
        {
            var minStockSetting = await _context.Settings.FindAsync("MinStockLevel");
            int minStock = minStockSetting != null ? int.Parse(minStockSetting.Value) : 10;

            return await _context.Products
                .Where(p => p.IsInventoryTracked && p.Quantity <= minStock)
                .ToListAsync();
        }
    }
}