using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.UI.ViewModels;
using System.Windows;

namespace QuanLyQuanBida.UI.Views
{
    public partial class PaymentWindow : Window
    {
        public PaymentWindow(InvoiceDto invoice)
        {
            InitializeComponent();

            if (DataContext is PaymentViewModel viewModel)
            {
                viewModel.Invoice = invoice;
            }
        }
    }
}