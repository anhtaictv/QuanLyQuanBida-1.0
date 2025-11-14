using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using TableEntity = QuanLyQuanBida.Core.Entities.Table;

namespace QuanLyQuanBida.Application.Services;
public class TableService : ITableService
{
    private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;

    public TableService(IDbContextFactory<QuanLyBidaDbContext> contextFactory) 
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<TableEntity>> GetAllTablesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Tables.ToListAsync();
    }
}