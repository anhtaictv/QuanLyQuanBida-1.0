using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyQuanBida.Core.Interfaces
{
    public interface ISettingService
    {
        Task<T> GetSettingAsync<T>(string key, T? defaultValue = default);
        Task<bool> SetSettingAsync<T>(string key, T value);
        Task<Dictionary<string, string>> GetAllSettingsAsync();
    }
}
