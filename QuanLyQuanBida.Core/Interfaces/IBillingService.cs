using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyQuanBida.Core.DTOs;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IBillingService
    {
        Task<InvoiceDto> GenerateInvoiceAsync(int sessionId);
        Task<bool> ProcessPaymentAsync(int invoiceId, PaymentDto payment);
    }
}
