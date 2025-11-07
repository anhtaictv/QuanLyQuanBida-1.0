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
    private readonly ICurrentUserService _currentUserService; // (1) Thêm dòng này

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // (2) Cập nhật constructor để nhận ICurrentUserService
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

        User? loggedInUser = await _authService.LoginAsync(Username, Password);

        if (loggedInUser != null)
        {
            // (3) Lưu thông tin user đã đăng nhập vào service
            _currentUserService.CurrentUser = loggedInUser;

            // Đăng nhập thành công!
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
}