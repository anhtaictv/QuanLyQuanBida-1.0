using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.UI.ViewModels;
using System.Windows;

namespace QuanLyQuanBida.UI.Views
{
    public partial class TableManagementView : Window
    {
        public TableManagementView()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<TableManagementViewModel>();
        }
    }
}