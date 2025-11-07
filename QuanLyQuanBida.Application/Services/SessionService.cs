using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services;

public class SessionService : ISessionService
{
    private readonly QuanLyBidaDbContext _context;

    public SessionService(QuanLyBidaDbContext context)
    {
        _context = context;
    }

    public async Task<Session?> StartSessionAsync(int tableId, int userId)
    {
        // 1. Kiểm tra xem bàn có đang trống không
        var table = await _context.Tables.FirstOrDefaultAsync(t => t.Id == tableId);
        if (table == null || table.Status != "Free")
        {
            return null; // Bàn không tồn tại hoặc không trống
        }

        // 2. Tạo một phiên chơi mới
        var newSession = new Session
        {
            TableId = tableId,
            UserOpenId = userId,
            StartAt = DateTime.UtcNow,
            Status = "Started"
        };

        _context.Sessions.Add(newSession);

        // 3. Cập nhật trạng thái bàn
        table.Status = "Occupied";

        // 4. Lưu thay đổi vào database
        await _context.SaveChangesAsync();

        return newSession;
    }
}