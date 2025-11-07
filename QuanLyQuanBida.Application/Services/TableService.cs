using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using TableEntity = QuanLyQuanBida.Core.Entities.Table;

namespace QuanLyQuanBida.Application.Services;

public class TableService : ITableService
{
    private readonly QuanLyBidaDbContext _context;

    public TableService(QuanLyBidaDbContext context)
    {
        _context = context;
    }

    public async Task<List<TableEntity>> GetAllTablesAsync()
    {
        return await _context.Tables.ToListAsync();
    }
}