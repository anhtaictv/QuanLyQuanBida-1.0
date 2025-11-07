using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Core.Interfaces;

public interface IAuthService
{
    // Phương thức đăng nhập, trả về thông tin User nếu thành công, null nếu thất bại
    Task<User?> LoginAsync(string username, string password);
    Task<bool> CreateUserAsync(string username, string password, string fullName, int roleId);
}