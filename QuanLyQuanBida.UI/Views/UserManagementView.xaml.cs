using System.Windows;
using QuanLyQuanBida.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace QuanLyQuanBida.UI.Views
{
    public partial class UserManagementView : Window
    {
        public UserManagementView()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<UserManagementViewModel>();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is UserManagementViewModel viewModel)
            {
                viewModel.UserForm.Password = ((System.Windows.Controls.PasswordBox)sender).Password;
            }
        }
    }
}