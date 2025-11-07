using System.Windows;
using QuanLyQuanBida.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace QuanLyQuanBida.UI.Views
{
    public partial class ReportsView : Window
    {
        public ReportsView()
        {
            InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ReportsViewModel>();
        }
    }
}