using System.Windows;
using QuanLyQuanBida.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace QuanLyQuanBida.UI.Views
{
    public partial class BackupWindow : Window
    {
        public BackupWindow()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<BackupViewModel>();
        }
    }
}