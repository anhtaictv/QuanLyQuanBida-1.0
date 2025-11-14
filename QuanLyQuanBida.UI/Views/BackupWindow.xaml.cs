using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.UI.ViewModels;
using System.Windows;

namespace QuanLyQuanBida.UI.Views
{
    public partial class BackupWindow : Window
    {
        public BackupWindow()
        {
            InitializeComponent();
            // Dòng này sẽ thay thế cho khối XAML bạn vừa xóa:
            this.DataContext = App.Services.GetRequiredService<BackupViewModel>();
        }
    }
}