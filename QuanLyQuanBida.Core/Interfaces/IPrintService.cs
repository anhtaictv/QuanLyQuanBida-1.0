using QuanLyQuanBida.Core.DTOs;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IPrintService
    {
        Task<bool> PrintInvoiceAsync(InvoiceDto invoice);
        Task<bool> PrintReportAsync<T>(List<T> data, string reportTitle);
    }
}