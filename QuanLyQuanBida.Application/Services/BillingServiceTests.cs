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
using System.Threading;

namespace QuanLyQuanBida.Tests.Unit.Services
{
    public class BillingServiceTests
    {
        private readonly DbContextOptions<QuanLyBidaDbContext> _dbOptions;
        private readonly Mock<IDbContextFactory<QuanLyBidaDbContext>> _mockContextFactory;
        private readonly Mock<ISettingService> _mockSettingService;
        private readonly Mock<IRateService> _mockRateService;
        private readonly Mock<ICustomerService> _mockCustomerService;

        public BillingServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<QuanLyBidaDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockContextFactory = new Mock<IDbContextFactory<QuanLyBidaDbContext>>();
            _mockSettingService = new Mock<ISettingService>();
            _mockRateService = new Mock<IRateService>();
            _mockCustomerService = new Mock<ICustomerService>();
        }

        private async Task SetupDatabaseWithDataAsync()
        {
            using (var context = new QuanLyBidaDbContext(_dbOptions))
            {
                var table = new Table { Id = 1, Name = "Bàn 1", Status = "Free", Code = "B1" };
                var user = new User { Id = 1, Username = "testuser", FullName = "Test User", RoleId = 1 };
                var product = new Product { Id = 1, Name = "Coca Cola", Price = 15000, Category = "Drink", Unit = "Chai" };
                var rate = new Rate { Id = 1, Name = "Giờ chuẩn", PricePerHour = 60000 };

                context.Tables.Add(table);
                context.Users.Add(user);
                context.Products.Add(product);
                context.Rates.Add(rate);
                await context.SaveChangesAsync();

                var session = new Session
                {
                    Id = 1,
                    TableId = 1,
                    UserOpenId = 1,
                    StartAt = DateTime.Now.AddHours(-2),
                    EndAt = DateTime.Now,
                    Status = "Finished",
                    TotalMinutes = 120,
                    RateId = 1
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
            }

            _mockContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QuanLyBidaDbContext(_dbOptions));
        }

        [Fact]
        public async Task GenerateInvoiceAsync_ShouldCalculateCorrectTotals()
        {
            // Arrange
            await SetupDatabaseWithDataAsync();

            _mockSettingService.Setup(s => s.GetSettingAsync<decimal>("TaxRate", 0.1m)).ReturnsAsync(0.1m);
            _mockSettingService.Setup(s => s.GetSettingAsync<decimal>("ServiceFeeRate", 0.05m)).ReturnsAsync(0.05m);
            _mockSettingService.Setup(s => s.GetSettingAsync<int>("TimeRoundingMinutes", 1)).ReturnsAsync(1);

            using (var context = new QuanLyBidaDbContext(_dbOptions))
            {
                _mockRateService.Setup(r => r.GetApplicableRateAsync(It.IsAny<DateTime>()))
                                .ReturnsAsync(await context.Rates.FindAsync(1));
            }


            var billingService = new BillingService(
                _mockContextFactory.Object,
                _mockSettingService.Object,
                _mockRateService.Object,
                _mockCustomerService.Object);

            // Act
            var result = await billingService.GenerateInvoiceAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(30000, result.OrderTotal);
            Assert.Equal(120000, result.TimeCharge);

            decimal subtotal = 120000 + 30000;
            Assert.Equal(subtotal, result.SubTotal);

            Assert.Equal(subtotal * 0.1m, result.Tax);
            Assert.Equal(subtotal * 0.05m, result.ServiceFee);
            Assert.Equal(subtotal + (subtotal * 0.1m) + (subtotal * 0.05m), result.Total);
            Assert.Equal(0, result.Discount);
        }
    }
}