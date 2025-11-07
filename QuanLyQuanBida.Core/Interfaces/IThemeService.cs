using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyQuanBida.Core.Interfaces
{
    public enum ThemeType
    {
        Light,
        Dark
    }

    public interface IThemeService
    {
        ThemeType CurrentTheme { get; }
        void SetTheme(ThemeType theme);
        event Action<ThemeType> ThemeChanged;
    }
}