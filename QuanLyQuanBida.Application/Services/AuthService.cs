using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using BCrypt.Net;

namespace QuanLyQuanBida.Application.Services;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;
    private readonly IPermissionService _permissionService;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        IDbContextFactory<QuanLyBidaDbContext> contextFactory,
        IPermissionService permissionService,
        ICurrentUserService currentUserService)
    {
        _contextFactory = contextFactory; 
        _permissionService = permissionService;
        _currentUserService = currentUserService;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // 1. Tìm user trong database dựa trên username
        var user = await context.Users
            .Include(u => u.Role) 
            .FirstOrDefaultAsync(u => u.Username == username);

        // 2. Nếu không tìm thấy user hoặc user đã bị vô hiệu hóa
        if (user == null || !user.IsActive)
        {
            return null;
        }
        // 3. Kiểm tra mật khẩu
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null; // Mật khẩu không đúng
        }
 
        // 4. Tải quyền và lưu thông tin user vào service
        _currentUserService.CurrentUser = user;
        _currentUserService.Permissions = await _permissionService.GetPermissionsByRoleIdAsync(user.RoleId);
        // 5. Trả về thông tin user
        return user;
    }

    public async Task<bool> CreateUserAsync(string username, string password, string fullName, int roleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (existingUser != null)
        {
            return false; 
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var newUser = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            FullName = fullName,
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(newUser);
        await context.SaveChangesAsync();
        return true;
    }
}