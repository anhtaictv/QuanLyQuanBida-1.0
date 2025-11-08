using System.Windows;
using QuanLyQuanBida.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace QuanLyQuanBida.UI.Views
{
    public partial class AddStockWindow : Window
    {
        public AddStockWindow()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<AddStockViewModel>();
        }
    }
}