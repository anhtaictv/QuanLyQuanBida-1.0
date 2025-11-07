using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Application.Services;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Interfaces;
using System.Windows;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class PaymentViewModel : ObservableObject
    {
        private readonly IBillingService _billingService;

        [ObservableProperty]
        private InvoiceDto _invoice = null!;

        public PaymentViewModel(IBillingService billingService, IPrintService printService)
        {
            _billingService = billingService;
            _printService = printService;
        }

        [RelayCommand]
        private async Task ProcessPayment()
        {
            if (Invoice == null) return;

            // Get payment method from ComboBox
            var paymentMethod = "Tiền mặt"; // Default

            // Create payment
            var payment = new PaymentDto
            {
                Method = paymentMethod,
                Amount = Invoice.Total,
                TransactionRef = ""
            };

            var success = await _billingService.ProcessPaymentAsync(Invoice.Id, payment);

            if (success)
            {
                MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                MessageBox.Show("Thanh toán thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task PrintInvoice()
        {
            if (Invoice == null) return;

            try
            {
                var success = await _printService.PrintInvoiceAsync(Invoice);
                if (success)
                {
                    MessageBox.Show("In hóa đơn thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể in hóa đơn. Vui lòng kiểm tra máy in.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}