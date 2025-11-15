using QuanLyQuanBida.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuanLyQuanBida.Core.DTOs.TableBatchCreateDto;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IReportService
    {
        Task<List<RevenueByDayDto>> GetRevenueByDayAsync(DateTime startDate, DateTime endDate);
        Task<List<RevenueByTableDto>> GetRevenueByTableAsync(DateTime startDate, DateTime endDate);
        Task<List<RevenueByProductDto>> GetRevenueByProductAsync(DateTime startDate, DateTime endDate);
        Task<List<CustomerDebtDto>> GetCustomerDebtsAsync();
        Task<List<InventoryReportDto>> GetInventoryReportAsync();
        Task<List<RevenueByHourDto>> GetRevenueByHourAsync(DateTime startDate, DateTime endDate);
        Task<List<RevenueByEmployeeDto>> GetRevenueByEmployeeAsync(DateTime startDate, DateTime endDate);
        Task<List<DetailedInvoiceReportDto>> GetDetailedInvoiceReportAsync(DateTime startDate, DateTime endDate);
    }
}
