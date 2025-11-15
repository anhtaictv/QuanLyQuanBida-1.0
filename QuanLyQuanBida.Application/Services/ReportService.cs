using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using static QuanLyQuanBida.Core.DTOs.TableBatchCreateDto;

namespace QuanLyQuanBida.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;

        public ReportService(IDbContextFactory<QuanLyBidaDbContext> contextFactory) 
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<RevenueByDayDto>> GetRevenueByDayAsync(DateTime startDate, DateTime endDate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            return await context.Invoices
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
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            return await context.Invoices
                .Include(i => i.Session.Table)
                .Where(i => i.CreatedAt.Date >= startDate.Date && i.CreatedAt.Date <= endDate.Date)
                .GroupBy(i => i.Session.Table)
                .Select(g => new RevenueByTableDto
                {
                    TableCode = g.Key.Code,
                    TableName = g.Key.Name,
                    Revenue = g.Sum(i => i.Total),
                    SessionsCount = g.Count(),
                    UtilizationRate = 0 
                })
                .OrderByDescending(r => r.Revenue)
                .ToListAsync();
        }

        public async Task<List<RevenueByProductDto>> GetRevenueByProductAsync(DateTime startDate, DateTime endDate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Orders
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
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            return await Task.FromResult(new List<CustomerDebtDto>());
        }

        public async Task<List<InventoryReportDto>> GetInventoryReportAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            return await context.Products
                .Where(p => p.IsInventoryTracked)
                .Select(p => new InventoryReportDto
                {
                    ProductName = p.Name,
                    Category = p.Category,
                    CurrentStock = p.Quantity, 
                    MinStock = 10, 
                    IsLowStock = p.Quantity <= 10 
                })
                .ToListAsync();
        }
        public async Task<List<RevenueByHourDto>> GetRevenueByHourAsync(DateTime startDate, DateTime endDate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Invoices
                .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate)
                .GroupBy(i => i.CreatedAt.Hour) 
                .Select(g => new RevenueByHourDto
                {
                    Hour = g.Key,
                    Revenue = g.Sum(i => i.Total),
                    SessionsCount = g.Count()
                })
                .OrderBy(r => r.Hour)
                .ToListAsync();
        }

        public async Task<List<RevenueByEmployeeDto>> GetRevenueByEmployeeAsync(DateTime startDate, DateTime endDate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Invoices
                .Include(i => i.Creator) 
                .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate)
                .GroupBy(i => i.Creator) 
                .Select(g => new RevenueByEmployeeDto
                {
                    EmployeeName = g.Key.FullName ?? g.Key.Username,
                    Revenue = g.Sum(i => i.Total),
                    SessionsCount = g.Count()
                })
                .OrderByDescending(r => r.Revenue)
                .ToListAsync();
        }
        public async Task<List<DetailedInvoiceReportDto>> GetDetailedInvoiceReportAsync(DateTime startDate, DateTime endDate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var invoices = await context.Invoices
                .AsNoTracking() 
                .Include(i => i.Session).ThenInclude(s => s.Table)
                .Include(i => i.Session).ThenInclude(s => s.Customer)
                .Include(i => i.Session).ThenInclude(s => s.Orders)
                .Include(i => i.Creator)
                .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return invoices.Select(i =>
            {
                var orderTotal = i.Session.Orders.Sum(o => o.Price * o.Quantity);
                return new DetailedInvoiceReportDto
                {
                    SoHoaDon = i.InvoiceNumber,
                    TenKhachHang = i.Session.Customer?.Name ?? "[Vãng lai]",
                    KhuVuc = i.Session.Table.Zone ?? "[Không rõ]", 
                    TenNhanVien = i.Creator.FullName ?? i.Creator.Username,
                    ThoiGianBatDau = i.Session.StartAt,
                    ThoiGianKetThuc = i.Session.EndAt,
                    TongSoPhut = i.Session.TotalMinutes,
                    TienGio = i.SubTotal - orderTotal,
                    TienDichVu = orderTotal,
                    GiamGia = i.Discount,
                    Thue = i.Tax,
                    TongCong = i.Total
                };
            }).ToList();
        }
    }
}
