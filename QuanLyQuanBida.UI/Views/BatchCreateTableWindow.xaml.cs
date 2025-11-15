using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.UI.ViewModels;
using System.Windows;

namespace QuanLyQuanBida.UI.Views
{
    public partial class BatchCreateTableWindow : Window
    {
        public BatchCreateTableWindow()
        {
            InitializeComponent();
            var viewModel = App.Services.GetRequiredService<BatchCreateTableViewModel>();
            this.DataContext = viewModel;
            // Gán hành động đóng cửa sổ
            viewModel.CloseAction = () => this.Close();
        }
    }
}