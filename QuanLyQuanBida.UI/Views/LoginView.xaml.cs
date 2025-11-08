using System.Windows;
using QuanLyQuanBida.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace QuanLyQuanBida.UI.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();

            // Lấy ViewModel từ DI container và gán làm DataContext
            this.DataContext = App.Services.GetRequiredService<LoginViewModel>();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Khi mật khẩu thay đổi, cập nhật vào ViewModel
            if (this.DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = ((System.Windows.Controls.PasswordBox)sender).Password;
            }
        }
    }
}