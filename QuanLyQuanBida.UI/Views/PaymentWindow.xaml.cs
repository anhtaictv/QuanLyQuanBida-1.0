using System.Windows;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace QuanLyQuanBida.UI.Views
{
    public partial class PaymentWindow : Window
    {
        public PaymentWindow(InvoiceDto invoice)
        {
            InitializeComponent();

            // Lấy ViewModel từ DI container
            var viewModel = App.Services.GetRequiredService<PaymentViewModel>();
            viewModel.Invoice = invoice; // Truyền dữ liệu vào ViewModel

            // Gán DataContext cho View
            this.DataContext = viewModel;
        }
    }
}