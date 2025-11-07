using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly QuanLyBidaDbContext _context;

        public CustomerService(QuanLyBidaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<Customer?> GetCustomerByPhoneAsync(string phone)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Phone == phone);
        }

        public async Task<Customer> CreateCustomerAsync(CustomerDto customerDto)
        {
            var newCustomer = new Customer
            {
                Name = customerDto.Name,
                Phone = customerDto.Phone,
                Address = customerDto.Address,
                Type = customerDto.Type,
                VipCardNumber = customerDto.VipCardNumber,
                Points = 0
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();
            return newCustomer;
        }

        public async Task<bool> UpdateCustomerAsync(CustomerDto customerDto)
        {
            var customer = await _context.Customers.FindAsync(customerDto.Id);
            if (customer == null)
                return false;

            customer.Name = customerDto.Name;
            customer.Phone = customerDto.Phone;
            customer.Address = customerDto.Address;
            customer.Type = customerDto.Type;
            customer.VipCardNumber = customerDto.VipCardNumber;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddPointsAsync(int customerId, int points)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
                return false;

            customer.Points += points;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}