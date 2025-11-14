using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Core.Interfaces;
public interface IRoleService
{
    // Phương thức để lấy tất cả các Role có trong DB
    Task<List<Role>> GetAllRolesAsync();
}