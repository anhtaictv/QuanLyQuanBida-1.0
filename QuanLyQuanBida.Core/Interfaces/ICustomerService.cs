using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.DTOs;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<Customer?> GetCustomerByPhoneAsync(string phone);
        Task<Customer> CreateCustomerAsync(CustomerDto customerDto);
        Task<bool> UpdateCustomerAsync(CustomerDto customerDto);
        Task<bool> DeleteCustomerAsync(int id);
        Task<bool> AddPointsAsync(int customerId, int points);
    }
}