using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IPermissionService
    {
        // Lấy danh sách các quyền (dạng string) của một RoleId
        Task<HashSet<string>> GetPermissionsByRoleIdAsync(int roleId);
    }
}