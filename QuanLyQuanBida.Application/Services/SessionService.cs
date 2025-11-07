using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;

namespace QuanLyQuanBida.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly QuanLyBidaDbContext _context;

        public SessionService(QuanLyBidaDbContext context)
        {
            _context = context;
        }

        // Đã sửa: Thêm rateId và trả về Session
        public async Task<Session?> StartSessionAsync(int tableId, int userId, int rateId)
        {
            var table = await _context.Tables.FirstOrDefaultAsync(t => t.Id == tableId);
            if (table == null || table.Status != "Free")
            {
                return null;
            }

            var newSession = new Session
            {
                TableId = tableId,
                UserOpenId = userId,
                StartAt = DateTime.UtcNow,
                Status = "Started"
            };

            _context.Sessions.Add(newSession);
            table.Status = "Occupied";

            await _context.SaveChangesAsync();
            return newSession;
        }

        public async Task<bool> PauseSessionAsync(int sessionId, int userId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null || session.Status != "Started")
            {
                return false;
            }

            session.Status = "Paused";
            session.PauseAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResumeSessionAsync(int sessionId, int userId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null || session.Status != "Paused")
            {
                return false;
            }

            session.Status = "Started";
            session.ResumeAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Session?> CloseSessionAsync(int sessionId, int userId)
        {
            var session = await _context.Sessions
                .Include(s => s.Table)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null || session.Status == "Finished")
            {
                return null;
            }

            session.EndAt = DateTime.UtcNow;
            session.Status = "Finished";

            var totalDuration = session.EndAt.Value - session.StartAt;
            if (session.PauseAt.HasValue && session.ResumeAt.HasValue)
            {
                totalDuration = totalDuration - (session.ResumeAt.Value - session.PauseAt.Value);
            }

            session.TotalMinutes = (int)totalDuration.TotalMinutes;
            session.Table.Status = "Free";

            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<bool> MoveSessionAsync(int sessionId, int targetTableId, int userId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            var targetTable = await _context.Tables.FindAsync(targetTableId);

            if (session == null || targetTable == null || targetTable.Status != "Free")
            {
                return false;
            }

            var currentTable = await _context.Tables.FindAsync(session.TableId);
            if (currentTable != null)
            {
                currentTable.Status = "Free";
            }

            session.TableId = targetTableId;
            targetTable.Status = "Occupied";

            await _context.SaveChangesAsync();
            return true;
        }

        // Thêm phương thức mới
        public async Task<Session?> GetActiveSessionByTableIdAsync(int tableId)
        {
            return await _context.Sessions
                .Where(s => s.TableId == tableId && s.Status != "Finished")
                .FirstOrDefaultAsync();
        }
    }
}