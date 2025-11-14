using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;

        public PermissionService(IDbContextFactory<QuanLyBidaDbContext> contextFactory) 
        {
            _contextFactory = contextFactory; 
        }

        public async Task<HashSet<string>> GetPermissionsByRoleIdAsync(int roleId)
        {
            return await Task.FromResult(GetHardCodedPermissions(roleId));
        }

        private HashSet<string> GetHardCodedPermissions(int roleId)
        {
            var permissions = new HashSet<string>();

            if (roleId == 4) 
            {
                permissions.Add("OpenSession");
                permissions.Add("CloseSession");
                permissions.Add("CreateOrder");
                permissions.Add("ViewTables");
            }

            if (roleId == 3 || roleId == 2 || roleId == 1) 
            {
                permissions.UnionWith(GetHardCodedPermissions(4));
                permissions.Add("ProcessPayment");
                permissions.Add("PrintInvoice");
            }
            
            if (roleId == 2 || roleId == 1) 
            {
                permissions.Add("ViewReports");
                permissions.Add("ManageProducts");
                permissions.Add("ManageCustomers");
                permissions.Add("ManageInventory");
                permissions.Add("ManageRates");
                permissions.Add("ManageShifts");
                permissions.Add("RefundLimited");
                permissions.Add("ManageUsers");
            }

            if (roleId == 1) 
            {

                permissions.Add("ManageSettings");
                permissions.Add("ViewAuditLog");
                permissions.Add("BackupDatabase");
            }

            return permissions;
        }
    }
}