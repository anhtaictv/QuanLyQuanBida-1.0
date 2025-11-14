using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services;
public class RoleService : IRoleService
{
    private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;

    public RoleService(IDbContextFactory<QuanLyBidaDbContext> contextFactory) 
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync(); 
        return await context.Roles.ToListAsync();
    }
}