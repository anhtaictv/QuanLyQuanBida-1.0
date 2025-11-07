using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Core.Interfaces;

public interface ISessionService
{
    Task<Session?> StartSessionAsync(int tableId, int userId);
}