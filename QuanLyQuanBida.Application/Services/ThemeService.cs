using QuanLyQuanBida.Core.Interfaces;

namespace QuanLyQuanBida.Application.Services
{
    public class ThemeService : IThemeService
    {
        private ThemeType _currentTheme = ThemeType.Light;

        public ThemeType CurrentTheme => _currentTheme;

        public event Action<ThemeType>? ThemeChanged;

        public void SetTheme(ThemeType theme)
        {
            if (_currentTheme != theme)
            {
                _currentTheme = theme;
                ThemeChanged?.Invoke(theme);
            }
        }
    }
}