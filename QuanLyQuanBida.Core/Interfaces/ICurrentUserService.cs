using QuanLyQuanBida.Core.Entities;
using System.Collections.Generic; // <-- THÊM USING NÀY (nếu chưa có)

namespace QuanLyQuanBida.Core.Interfaces;

public interface ICurrentUserService
{
    User? CurrentUser { get; set; }

    // <-- THÊM MỚI -->
    HashSet<string> Permissions { get; set; }
    bool HasPermission(string permissionKey);
    // <-- KẾT THÚC THÊM MỚI -->
}