using System.Windows;
using QuanLyQuanBida.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace QuanLyQuanBida.UI.Views
{
    public partial class ProductManagementView : Window
    {
        public ProductManagementView()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ProductManagementViewModel>();
        }
    }
}