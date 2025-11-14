using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using System;

namespace QuanLyQuanBida.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;
        private readonly IAuditService _auditLog;

        public SessionService(
            IDbContextFactory<QuanLyBidaDbContext> contextFactory, 
            IAuditService auditLog) 
        {
            _contextFactory = contextFactory; 
            _auditLog = auditLog;
        }

        public async Task<Session?> StartSessionAsync(int tableId, int userId, int rateId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            var table = await context.Tables.FirstOrDefaultAsync(t => t.Id == tableId);
            if (table == null || table.Status != "Free")
            {
                return null;
            }
            var newSession = new Session
            {
                TableId = tableId,
                UserOpenId = userId,
                StartAt = DateTime.UtcNow,
                Status = "Started",
                RateId = rateId
            };
            context.Sessions.Add(newSession);
            table.Status = "Occupied";
            await context.SaveChangesAsync();
            await _auditLog.LogActionAsync(userId, "START_SESSION", "Sessions", newSession.Id, newValue: $"Mở phiên bàn {table.Name}");
            return newSession;
        }

        public async Task<bool> PauseSessionAsync(int sessionId, int userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            var session = await context.Sessions.FindAsync(sessionId);
            if (session == null || session.Status != "Started")
            {
                return false;
            }
            session.Status = "Paused";
            session.PauseAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            await _auditLog.LogActionAsync(userId, "PAUSE_SESSION", "Sessions", sessionId, newValue: "Tạm dừng phiên");
            return true;
        }

        public async Task<bool> ResumeSessionAsync(int sessionId, int userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            var session = await context.Sessions.FindAsync(sessionId);
            if (session == null || session.Status != "Paused")
            {
                return false;
            }
            session.Status = "Started";
            session.ResumeAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            await _auditLog.LogActionAsync(userId, "RESUME_SESSION", "Sessions", sessionId, newValue: "Tiếp tục phiên");
            return true;
        }

        public async Task<Session?> CloseSessionAsync(int sessionId, int userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            var session = await context.Sessions
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
                var pauseDuration = session.ResumeAt.Value - session.PauseAt.Value;
                totalDuration = totalDuration - pauseDuration;
            }
            session.TotalMinutes = (int)totalDuration.TotalMinutes;
            session.Table.Status = "Free";
            await context.SaveChangesAsync();
            await _auditLog.LogActionAsync(userId, "CLOSE_SESSION", "Sessions", sessionId, newValue: $"Đóng phiên bàn {session.Table.Name}");
            return session;
        }

        public async Task<bool> MoveSessionAsync(int sessionId, int targetTableId, int userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var session = await context.Sessions.FindAsync(sessionId);
            var targetTable = await context.Tables.FindAsync(targetTableId);
            if (session == null || targetTable == null || targetTable.Status != "Free")
            {
                return false;
            }
            var currentTable = await context.Tables.FindAsync(session.TableId);
            string oldTableName = currentTable?.Name ?? "Unknown";
            if (currentTable != null)
            {
                currentTable.Status = "Free";
            }
            session.TableId = targetTableId;
            targetTable.Status = "Occupied";
            await context.SaveChangesAsync();
            await _auditLog.LogActionAsync(userId, "MOVE_SESSION", "Sessions", sessionId,
                oldValue: $"Bàn {oldTableName}",
                newValue: $"Bàn {targetTable.Name}");
            return true;
        }

        public async Task<Session?> GetActiveSessionByTableIdAsync(int tableId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(); 
            return await context.Sessions
                .Where(s => s.TableId == tableId && s.Status != "Finished")
                .FirstOrDefaultAsync();
        }
    }
}