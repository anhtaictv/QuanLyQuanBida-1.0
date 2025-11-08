using System.Windows;
using QuanLyQuanBida.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace QuanLyQuanBida.UI.Views
{
    public partial class InventoryManagementView : Window
    {
        public InventoryManagementView()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<InventoryManagementViewModel>();
        }
    }
}