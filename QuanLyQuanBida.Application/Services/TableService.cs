using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using QuanLyQuanBida.Core.DTOs; // SỬA: Thêm DTO
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

    // --- BỔ SUNG CÁC PHƯƠNG THỨC MỚI ---

    public async Task<TableEntity?> GetTableByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Tables.FindAsync(id);
    }

    public async Task<TableEntity> CreateTableAsync(TableDto tableDto)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var newTable = new TableEntity
        {
            Code = tableDto.Code,
            Name = tableDto.Name,
            Zone = tableDto.Zone,
            Seats = tableDto.Seats,
            Status = tableDto.Status ?? "Free", // Mặc định là "Free"
            Note = tableDto.Note
        };
        context.Tables.Add(newTable);
        await context.SaveChangesAsync();
        return newTable;
    }

    public async Task<bool> UpdateTableAsync(TableDto tableDto)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var table = await context.Tables.FindAsync(tableDto.Id);
        if (table == null) return false;

        table.Code = tableDto.Code;
        table.Name = tableDto.Name;
        table.Zone = tableDto.Zone;
        table.Seats = tableDto.Seats;
        table.Status = tableDto.Status ?? table.Status;
        table.Note = tableDto.Note;

        context.Tables.Update(table);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTableAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var table = await context.Tables.FindAsync(id);
        if (table == null) return false;

        // KIỂM TRA: Không cho xóa bàn đang có phiên
        if (await context.Sessions.AnyAsync(s => s.TableId == id && s.Status != "Finished"))
        {
            return false; // Trả về false nếu bàn đang được sử dụng
        }

        context.Tables.Remove(table);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateTablesAsync(List<TableDto> tables)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Bắt đầu một giao dịch CSDL
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            foreach (var tableDto in tables)
            {
                var newTable = new TableEntity
                {
                    Code = tableDto.Code,
                    Name = tableDto.Name,
                    Zone = tableDto.Zone,
                    Seats = tableDto.Seats,
                    Status = "Free",
                    Note = tableDto.Note
                };
                context.Tables.Add(newTable);
            }
            await context.SaveChangesAsync(); // Lưu tất cả thay đổi
            await transaction.CommitAsync(); // Hoàn tất giao dịch
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(); // Hoàn tác nếu có lỗi
            return false;
        }
    }
}