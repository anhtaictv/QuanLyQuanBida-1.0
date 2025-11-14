using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;

        public UserService(IDbContextFactory<QuanLyBidaDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> UpdateUserAsync(UserDto userDto)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.Users.FindAsync(userDto.Id);
            if (user == null) return false;

            // Kiểm tra username trùng lặp (nếu thay đổi)
            if (user.Username != userDto.Username && await context.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                return false; // Username đã tồn tại
            }

            user.Username = userDto.Username;
            user.FullName = userDto.FullName;
            user.Phone = userDto.Phone;
            user.RoleId = userDto.RoleId;
            user.IsActive = userDto.IsActive;

            // Chỉ cập nhật mật khẩu nếu được cung cấp
            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            }

            context.Users.Update(user);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.Users.FindAsync(id);
            if (user == null) return false;

            // Không cho xóa user admin
            if (user.Username == "admin") return false;

            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.Users.FindAsync(userId);
            if (user == null) return false;

            // Kiểm tra mật khẩu cũ
            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                return false; // Mật khẩu cũ không đúng
            }

            // Cập nhật mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await context.SaveChangesAsync();
            return true;
        }
    }
}