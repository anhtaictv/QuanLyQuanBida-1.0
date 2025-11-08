using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using QuanLyQuanBida.Core.Entities;


namespace QuanLyQuanBida.Application.Services
{
    public class SettingService : ISettingService
    {
        private readonly QuanLyBidaDbContext _context;

        public SettingService(QuanLyBidaDbContext context)
        {
            _context = context;
        }

        public async Task<T> GetSettingAsync<T>(string key, T defaultValue = default)
        {
            var setting = await _context.Settings.FindAsync(key);
            if (setting == null)
                return defaultValue;

            try
            {
                return (T)Convert.ChangeType(setting.Value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public async Task<bool> SetSettingAsync<T>(string key, T value)
        {
            var setting = await _context.Settings.FindAsync(key);

            if (setting == null)
            {
                setting = new Setting { Key = key, Value = value.ToString() };
                _context.Settings.Add(setting);
            }
            else
            {
                setting.Value = value.ToString();
                _context.Settings.Update(setting);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<string, string>> GetAllSettingsAsync()
        {
            var settings = await _context.Settings.ToListAsync();
            return settings.ToDictionary(s => s.Key, s => s.Value);
        }
    }
}
