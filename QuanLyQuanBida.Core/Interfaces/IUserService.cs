using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserAsync(UserDto userDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }
}