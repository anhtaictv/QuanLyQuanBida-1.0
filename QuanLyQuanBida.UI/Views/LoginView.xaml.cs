using System.Windows;
using QuanLyQuanBida.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.Core.Interfaces;

namespace QuanLyQuanBida.UI.Views
{
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
                viewModel.Password = ((System.Windows.Controls.PasswordBox)sender).Password;
            }
        }
        private void LoginView_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            var currentUserService = App.Services.GetRequiredService<ICurrentUserService>();

            if (currentUserService.CurrentUser == null)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}