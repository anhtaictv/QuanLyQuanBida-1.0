using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace QuanLyQuanBida.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // === THÊM THUỘC TÍNH NÀY ===
    [ObservableProperty]
    private bool _isRememberMe = false;
    // ========================

    public LoginViewModel(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Vui lòng nhập tên đăng nhập và mật khẩu.";
            return;
        }

        try
        {
            User? loggedInUser = await _authService.LoginAsync(Username, Password);

            if (loggedInUser != null)
            {
                // Lưu thông tin user đã đăng nhập vào service
                _currentUserService.CurrentUser = loggedInUser;

                // Đóng cửa sổ Login
                App.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext is LoginViewModel)?.Close();

                // Mở cửa sổ chính (MainWindow)
                var mainWindow = App.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            else
            {
                // Đăng nhập thất bại
                ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
            }
        }
        catch (Exception ex)
        {
            // Hiển thị lỗi nếu có (ví dụ: lỗi kết nối DB)
            ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
        }
    }
}