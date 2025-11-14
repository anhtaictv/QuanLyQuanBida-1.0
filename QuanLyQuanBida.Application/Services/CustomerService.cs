using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;

        public CustomerService(IDbContextFactory<QuanLyBidaDbContext> contextFactory) 
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            return await context.Customers.ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            return await context.Customers.FindAsync(id);
        }

        public async Task<Customer?> GetCustomerByPhoneAsync(string phone)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            return await context.Customers.FirstOrDefaultAsync(c => c.Phone == phone);
        }

        public async Task<Customer> CreateCustomerAsync(CustomerDto customerDto)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            var newCustomer = new Customer
            {
                Name = customerDto.Name,
                Phone = customerDto.Phone,
                Address = customerDto.Address,
                Type = customerDto.Type,
                VipCardNumber = customerDto.VipCardNumber,
                Points = 0
            };
            context.Customers.Add(newCustomer);
            await context.SaveChangesAsync();
            return newCustomer;
        }

        public async Task<bool> UpdateCustomerAsync(CustomerDto customerDto)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            var customer = await context.Customers.FindAsync(customerDto.Id);
  
            if (customer == null)
                return false;
            customer.Name = customerDto.Name;
            customer.Phone = customerDto.Phone;
            customer.Address = customerDto.Address;
            customer.Type = customerDto.Type;
            customer.VipCardNumber = customerDto.VipCardNumber;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var customer = await context.Customers.FindAsync(id);

            if (customer == null)
                return false;
            context.Customers.Remove(customer);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddPointsAsync(int customerId, int points)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            var customer = await context.Customers.FindAsync(customerId);
            if (customer == null)
                return false;
            customer.Points += points;
            await context.SaveChangesAsync();
            return true;
        }
    }
}