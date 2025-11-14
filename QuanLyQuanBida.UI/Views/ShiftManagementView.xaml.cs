using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.UI.ViewModels;
using System.Windows;

namespace QuanLyQuanBida.UI.Views
{
    public partial class ShiftManagementView : Window
    {
        public ShiftManagementView()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ShiftManagementViewModel>();
        }
    }
}