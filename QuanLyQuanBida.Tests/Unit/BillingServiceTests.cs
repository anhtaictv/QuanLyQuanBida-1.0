using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Application.Services;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyQuanBida.Tests.Unit.Services
{
    public class BillingServiceTests
    {
        [Fact]
        public async Task GenerateInvoiceAsync_ShouldCalculateCorrectTotals()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuanLyBidaDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new QuanLyBidaDbContext(options))
            {
                // Setup test data
                var table = new Table { Id = 1, Name = "Bàn 1", Status = "Free" };
                var user = new User { Id = 1, Username = "testuser", FullName = "Test User" };
                var product = new Product { Id = 1, Name = "Coca Cola", Price = 15000 };

                context.Tables.Add(table);
                context.Users.Add(user);
                context.Products.Add(product);
                await context.SaveChangesAsync();

                var session = new Session
                {
                    Id = 1,
                    TableId = 1,
                    UserOpenId = 1,
                    StartAt = DateTime.Now.AddHours(-2),
                    EndAt = DateTime.Now,
                    Status = "Finished",
                    TotalMinutes = 120
                };

                context.Sessions.Add(session);
                await context.SaveChangesAsync();

                var order = new Order
                {
                    Id = 1,
                    SessionId = 1,
                    ProductId = 1,
                    Quantity = 2,
                    Price = 15000
                };

                context.Orders.Add(order);
                await context.SaveChangesAsync();

                var mockSettingService = new Mock<ISettingService>();
                mockSettingService.Setup(s => s.GetSettingAsync<decimal>("TaxRate", 0.1m)).ReturnsAsync(0.1m);
                mockSettingService.Setup(s => s.GetSettingAsync<decimal>("ServiceFeeRate", 0.05m)).ReturnsAsync(0.05m);

                var billingService = new BillingService(context, mockSettingService.Object);

                // Act
                var result = await billingService.GenerateInvoiceAsync(1);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(30000, result.OrderTotal); // 2 * 15000
                Assert.True(result.TimeCharge > 0); // Should have time charge
                Assert.Equal(result.OrderTotal + result.TimeCharge, result.SubTotal);
                Assert.Equal(result.SubTotal * 0.1m, result.Tax);
                Assert.Equal(result.SubTotal * 0.05m, result.ServiceFee);
                Assert.Equal(result.SubTotal + result.Tax + result.ServiceFee, result.Total);
            }
        }
    }
}