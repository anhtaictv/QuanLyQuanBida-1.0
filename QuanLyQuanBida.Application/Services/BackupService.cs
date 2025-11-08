using Microsoft.Data.SqlClient;
using QuanLyQuanBida.Core.Interfaces;
using System.IO;

namespace QuanLyQuanBida.Application.Services
{
    public class BackupService : IBackupService
    {
        private readonly string _connectionString;

        public BackupService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> BackupDatabaseAsync(string backupPath)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var dbName = connection.Database;
                    var backupFileName = $"{dbName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                    var fullBackupPath = Path.Combine(backupPath, backupFileName);

                    var command = connection.CreateCommand();
                    command.CommandText = $"BACKUP DATABASE [{dbName}] TO DISK = '{fullBackupPath}' WITH INIT, NAME = '{dbName}-Full Database Backup', STATS = 10";

                    await command.ExecuteNonQueryAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log lỗi
                return false;
            }
        }

        public async Task<bool> RestoreDatabaseAsync(string backupPath)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var dbName = connection.Database;

                    // Đặt database ở single user mode để restore
                    var setSingleUserCommand = connection.CreateCommand();
                    setSingleUserCommand.CommandText = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    await setSingleUserCommand.ExecuteNonQueryAsync();

                    // Restore database
                    var restoreCommand = connection.CreateCommand();
                    restoreCommand.CommandText = $"RESTORE DATABASE [{dbName}] FROM DISK = '{backupPath}' WITH REPLACE";
                    await restoreCommand.ExecuteNonQueryAsync();

                    // Đặt lại multi user mode
                    var setMultiUserCommand = connection.CreateCommand();
                    setMultiUserCommand.CommandText = $"ALTER DATABASE [{dbName}] SET MULTI_USER";
                    await setMultiUserCommand.ExecuteNonQueryAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log lỗi
                return false;
            }
        }
    }
}