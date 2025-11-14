using System.Windows;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace QuanLyQuanBida.UI.Views
{
    public partial class PaymentWindow : Window
    {
        public PaymentWindow()
        {
            InitializeComponent();

            // Lấy ViewModel từ DI container
            var viewModel = App.Services.GetRequiredService<PaymentViewModel>();
            this.DataContext = viewModel;

            // Gán hành động đóng cửa sổ
            viewModel.CloseAction = () => this.Close();
        }
    }
}