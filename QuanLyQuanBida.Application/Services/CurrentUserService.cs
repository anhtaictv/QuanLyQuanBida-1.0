using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;

namespace QuanLyQuanBida.Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public User? CurrentUser { get; set; }

        // <-- THÊM MỚI -->
        public HashSet<string> Permissions { get; set; } = new HashSet<string>();

        public bool HasPermission(string permissionKey)
        {
            // Owner (RoleId = 1) luôn có mọi quyền
            if (CurrentUser?.RoleId == 1) return true;

            return Permissions.Contains(permissionKey);
        }
        // <-- KẾT THÚC THÊM MỚI -->
    }
}