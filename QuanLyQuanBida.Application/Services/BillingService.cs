using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
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

        private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;
        private readonly ISettingService _settingService;
        private readonly IRateService _rateService;
        private readonly ICustomerService _customerService;

        public BillingService(
            IDbContextFactory<QuanLyBidaDbContext> contextFactory, 
            ISettingService settingService,
            IRateService rateService,
            ICustomerService customerService)
        {
            _contextFactory = contextFactory; 
            _settingService = settingService;
            _rateService = rateService;
            _customerService = customerService;
        }

        public async Task<InvoiceDto> GenerateInvoiceAsync(int sessionId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var session = await context.Sessions
                .Include(s => s.Table)
                .Include(s => s.Orders)
                    .ThenInclude(o => o.Product) 
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null || !session.EndAt.HasValue) throw new ArgumentException("Session is not closed.");

            // 1. Tính toán Giờ chơi và Tiền giờ
            var rate = await _rateService.GetApplicableRateAsync(session.StartAt)
                ?? new Rate { PricePerHour = 100000 };
            int roundingRule = await _settingService.GetSettingAsync<int>("TimeRoundingMinutes", 1);
            decimal timeCharge = CalculateTimeCharge(session.TotalMinutes, rate.PricePerHour, roundingRule);

            // 2. Tính tổng Order
            decimal orderTotal = session.Orders.Sum(o => o.Price * o.Quantity);

            // 3. Tính Subtotal
            decimal subtotal = timeCharge + orderTotal;

            // 4. Áp dụng Discount
            decimal discountRate = 0m;
            if (session.CustomerId.HasValue)
            {
                var customer = await _customerService.GetCustomerByIdAsync(session.CustomerId.Value);
                if (customer != null && customer.Type == "VIP")
                {
                    discountRate = await _settingService.GetSettingAsync<decimal>($"VipDiscountRate_{customer.VipCardNumber}", 0.05m);
                }
            }
            decimal discount = subtotal * discountRate;

            // 5. Tính Thuế và Phí Dịch vụ
            var taxRate = await _settingService.GetSettingAsync<decimal>("TaxRate", 0.1m);
            var serviceFeeRate = await _settingService.GetSettingAsync<decimal>("ServiceFeeRate", 0.05m);
            decimal totalAfterDiscount = subtotal - discount;
            decimal tax = totalAfterDiscount * taxRate;
            decimal serviceFee = totalAfterDiscount * serviceFeeRate;

            // 6. Tính Total
            decimal total = totalAfterDiscount + tax + serviceFee;

            // Create invoice
            var invoice = new Invoice
            {
                SessionId = sessionId,
                InvoiceNumber = GenerateInvoiceNumber(),
                SubTotal = subtotal,
                Tax = tax,
                ServiceFee = serviceFee,
                Discount = discount,
                Total = total,
                PaidAmount = 0,
                PaymentMethod = "Pending",
                CreatedBy = session.UserOpenId,
                CreatedAt = DateTime.UtcNow
            };

            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

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
                Discount = discount,
                Total = total,
                Orders = session.Orders.Select(o => new OrderDto
                {
                    Id = o.Id,
                    ProductId = o.ProductId,
                    ProductName = o.Product.Name,
                    Quantity = o.Quantity,
                    Price = o.Price
                }).ToList()
            };
        }

        public async Task<bool> ProcessPaymentAsync(int invoiceId, PaymentDto payment)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); // SỬA

            var invoice = await context.Invoices.FindAsync(invoiceId);
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
            context.Payments.Add(paymentRecord);
            // Update invoice
            invoice.PaidAmount += payment.Amount;
            invoice.PaymentMethod = payment.Method;
            // Mark as fully paid if applicable
            if (invoice.PaidAmount >= invoice.Total)
            {
                invoice.PaymentMethod = "Paid";
            }
            await context.SaveChangesAsync();
            return true;
        }

        private decimal CalculateTimeCharge(int totalMinutes, decimal pricePerHour, int roundingRuleMinutes)
        {

            if (totalMinutes <= 0) return 0m;
            decimal minutesToBill = totalMinutes;
            if (roundingRuleMinutes > 1)
            {
                minutesToBill = Math.Ceiling((decimal)totalMinutes / roundingRuleMinutes) * roundingRuleMinutes;
            }
            decimal pricePerMinute = pricePerHour / 60m;
            return minutesToBill * pricePerMinute;
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}";
        }
    }
}