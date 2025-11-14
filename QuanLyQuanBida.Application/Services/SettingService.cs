using Microsoft.EntityFrameworkCore;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Infrastructure.Data.Context;
using QuanLyQuanBida.Core.Entities;

namespace QuanLyQuanBida.Application.Services
{
    public class SettingService : ISettingService
    {
        private readonly IDbContextFactory<QuanLyBidaDbContext> _contextFactory;

        public SettingService(IDbContextFactory<QuanLyBidaDbContext> contextFactory) 
        {
            _contextFactory = contextFactory; 
        }

        public async Task<T> GetSettingAsync<T>(string key, T defaultValue = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var setting = await context.Settings.FindAsync(key);
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
            await using var context = await _contextFactory.CreateDbContextAsync();
            var setting = await context.Settings.FindAsync(key);
            if (setting == null)
            {
                setting = new Setting { Key = key, Value = value.ToString() };
                context.Settings.Add(setting);
            }
            else
            {
                setting.Value = value.ToString();
                context.Settings.Update(setting);
            }
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<string, string>> GetAllSettingsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var settings = await context.Settings.ToListAsync();
            return settings.ToDictionary(s => s.Key, s => s.Value);
        }
    }
}