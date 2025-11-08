using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Entities;
using Microsoft.EntityFrameworkCore;


namespace QuanLyQuanBida.Application.Services
{
    public class BillingService : IBillingService
    {
        private readonly QuanLyBidaDbContext _context;
        private readonly ISettingService _settingService; // We need to create this

        public BillingService(QuanLyBidaDbContext context, ISettingService settingService)
        {
            _context = context;
            _settingService = settingService;
        }

        public async Task<InvoiceDto> GenerateInvoiceAsync(int sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Table)
                .Include(s => s.Orders)
                .ThenInclude(o => o.Product)
                .Include(s => s.UserOpen)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                throw new ArgumentException("Session not found");
            }

            // Get rate for the session
            var rate = await GetApplicableRateAsync(session.StartAt);

            // Calculate time charge
            decimal timeCharge = CalculateTimeCharge(session.TotalMinutes, rate.PricePerHour);

            // Calculate order total
            decimal orderTotal = session.Orders.Sum(o => o.Price * o.Quantity);

            // Get settings
            var taxRate = await _settingService.GetSettingAsync<decimal>("TaxRate", 0.1m); // 10% default
            var serviceFeeRate = await _settingService.GetSettingAsync<decimal>("ServiceFeeRate", 0.05m); // 5% default

            // Calculate subtotal
            decimal subtotal = timeCharge + orderTotal;

            // Calculate tax and service fee
            decimal tax = subtotal * taxRate;
            decimal serviceFee = subtotal * serviceFeeRate;

            // Calculate total
            decimal total = subtotal + tax + serviceFee;

            // Create invoice
            var invoice = new Invoice
            {
                SessionId = sessionId,
                InvoiceNumber = GenerateInvoiceNumber(),
                SubTotal = subtotal,
                Tax = tax,
                ServiceFee = serviceFee,
                Discount = 0, // We'll implement discount later
                Total = total,
                PaidAmount = 0,
                PaymentMethod = "Pending",
                CreatedBy = session.UserOpenId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Return DTO
            return new InvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                TableName = session.Table.Name,
                StartTime = session.StartAt,
                EndTime = session.EndAt,
                DurationMinutes = session.TotalMinutes,
                TimeCharge = timeCharge,
                OrderTotal = orderTotal,
                SubTotal = subtotal,
                Tax = tax,
                ServiceFee = serviceFee,
                Total = total
            };
        }

        public async Task<bool> ProcessPaymentAsync(int invoiceId, PaymentDto payment)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                return false;
            }

            // Create payment record
            var paymentRecord = new Payment
            {
                InvoiceId = invoiceId,
                Method = payment.Method,
                Amount = payment.Amount,
                TransactionRef = payment.TransactionRef,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(paymentRecord);

            // Update invoice
            invoice.PaidAmount += payment.Amount;
            invoice.PaymentMethod = payment.Method;

            // Mark as fully paid if applicable
            if (invoice.PaidAmount >= invoice.Total)
            {
                invoice.PaymentMethod = "Paid";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<Rate> GetApplicableRateAsync(DateTime dateTime)
        {
            var time = TimeOnly.FromDateTime(dateTime);
            var isWeekend = dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;

            return await _context.Rates
                .Where(r =>
                    (r.StartTimeWindow <= time && r.EndTimeWindow >= time) &&
                    (r.IsWeekendRate == isWeekend || !r.IsWeekendRate))
                .FirstOrDefaultAsync() ?? new Rate { PricePerHour = 100 }; // Default rate
        }

        private decimal CalculateTimeCharge(int totalMinutes, decimal pricePerHour)
        {
            // Convert to hours and calculate
            decimal hours = totalMinutes / 60.0m;
            return hours * pricePerHour;
        }

        private string GenerateInvoiceNumber()
        {
            // Format: INV + YYYYMMDD + 4-digit sequence
            return $"INV{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}";
        }
    }

}
