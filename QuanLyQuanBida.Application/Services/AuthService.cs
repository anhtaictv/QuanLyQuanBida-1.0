using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using BCrypt.Net;

namespace QuanLyQuanBida.Application.Services;

public class AuthService : IAuthService
{
    private readonly QuanLyBidaDbContext _context;

    public AuthService(QuanLyBidaDbContext context)
    {
        _context = context;
    }

    public async Task<User?> LoginAsync(string username, string password)

    {
        // 1. Tìm user trong database dựa trên username
        var user = await _context.Users
                                .Include(u => u.Role) // Lấy luôn thông tin Role
                                .FirstOrDefaultAsync(u => u.Username == username);

        // 2. Nếu không tìm thấy user hoặc user đã bị vô hiệu hóa
        if (user == null || !user.IsActive)
        {
            return null;
        }

        // 3. Kiểm tra mật khẩu
        // BCrypt.Verify(password, user.PasswordHash) sẽ so sánh mật khẩu người dùng nhập
        // với chuỗi hash đã lưu trong database.
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null; // Mật khẩu không đúng
        }

        // 4. Nếu mọi thứ đúng, trả về thông tin user
        return user;
    }
    public async Task<bool> CreateUserAsync(string username, string password, string fullName, int roleId)
    {
        // Kiểm tra xem username đã tồn tại chưa
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (existingUser != null)
        {
            return false; // Username đã tồn tại
        }

        // Băm mật khẩu
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // Tạo đối tượng user mới
        var newUser = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            FullName = fullName,
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Thêm vào database
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return true;
    }
}