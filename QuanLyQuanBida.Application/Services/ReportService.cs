using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly QuanLyBidaDbContext _context;

        public ReportService(QuanLyBidaDbContext context)
        {
            _context = context;
        }

        public async Task<List<RevenueByDayDto>> GetRevenueByDayAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Invoices
                .Where(i => i.CreatedAt.Date >= startDate.Date && i.CreatedAt.Date <= endDate.Date)
                .GroupBy(i => i.CreatedAt.Date)
                .Select(g => new RevenueByDayDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(i => i.Total),
                    SessionsCount = g.Count()
                })
                .OrderBy(r => r.Date)
                .ToListAsync();
        }

        public async Task<List<RevenueByTableDto>> GetRevenueByTableAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Invoices
                .Include(i => i.Session.Table)
                .Where(i => i.CreatedAt.Date >= startDate.Date && i.CreatedAt.Date <= endDate.Date)
                .GroupBy(i => i.Session.Table)
                .Select(g => new RevenueByTableDto
                {
                    TableCode = g.Key.Code,
                    TableName = g.Key.Name,
                    Revenue = g.Sum(i => i.Total),
                    SessionsCount = g.Count(),
                    UtilizationRate = 0 // Will be calculated separately
                })
                .OrderByDescending(r => r.Revenue)
                .ToListAsync();
        }

        public async Task<List<RevenueByProductDto>> GetRevenueByProductAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Session)
                .Where(o => o.Session.StartAt.Date >= startDate.Date && o.Session.StartAt.Date <= endDate.Date)
                .GroupBy(o => o.Product)
                .Select(g => new RevenueByProductDto
                {
                    ProductName = g.Key.Name,
                    Category = g.Key.Category,
                    Quantity = g.Sum(o => o.Quantity),
                    Revenue = g.Sum(o => o.Price * o.Quantity)
                })
                .OrderByDescending(r => r.Revenue)
                .ToListAsync();
        }

        public async Task<List<CustomerDebtDto>> GetCustomerDebtsAsync()
        {
            // Assuming debt is tracked in a separate table or as a field in Customer/Invoice
            // For now, we'll return an empty list as this feature needs more specification
            return await Task.FromResult(new List<CustomerDebtDto>());
        }

        public async Task<List<InventoryReportDto>> GetInventoryReportAsync()
        {
            return await _context.Products
                .Where(p => p.IsInventoryTracked)
                .Select(p => new InventoryReportDto
                {
                    ProductName = p.Name,
                    Category = p.Category,
                    CurrentStock = 0, // Will be calculated from InventoryTransactions
                    MinStock = 10, // Default min stock, should be a setting
                    IsLowStock = false // Will be calculated
                })
                .ToListAsync();
        }
    }
}