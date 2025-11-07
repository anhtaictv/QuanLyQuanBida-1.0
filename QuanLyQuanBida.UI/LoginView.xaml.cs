using System.Windows;
using QuanLyQuanBida.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace QuanLyQuanBida.UI.Views;
public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<LoginViewModel>();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (this.DataContext is LoginViewModel viewModel)
        {
            // Khi mật khẩu thay đổi, cập nhật vào ViewModel
            viewModel.Password = ((System.Windows.Controls.PasswordBox)sender).Password;
        }
    }
}