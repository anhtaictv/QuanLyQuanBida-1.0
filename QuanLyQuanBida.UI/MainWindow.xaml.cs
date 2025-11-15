using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.UI.ViewModels;
using System.Windows;
// SỬA: Xóa using System.Windows.Input và các sự kiện click

namespace QuanLyQuanBida.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext được gán tự động từ App.Services
            this.DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
        }
    }

}