namespace QuanLyQuanBida.Core.Interfaces
{
    public interface IBackupService
    {
        Task<bool> BackupDatabaseAsync(string backupPath);
        Task<bool> RestoreDatabaseAsync(string backupPath);
    }
}