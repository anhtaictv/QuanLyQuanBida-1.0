using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyQuanBida.Core.DTOs;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IReportService
    {
        Task<List<RevenueByDayDto>> GetRevenueByDayAsync(DateTime startDate, DateTime endDate);
        Task<List<RevenueByTableDto>> GetRevenueByTableAsync(DateTime startDate, DateTime endDate);
        Task<List<RevenueByProductDto>> GetRevenueByProductAsync(DateTime startDate, DateTime endDate);
        Task<List<CustomerDebtDto>> GetCustomerDebtsAsync();
        Task<List<InventoryReportDto>> GetInventoryReportAsync();
    }
}
