using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.Core.DTOs;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class PaymentViewModel : ObservableObject
    {
        private readonly IBillingService _billingService;
        private readonly IPrintService _printService;

        [ObservableProperty]
        private InvoiceDto _invoice = null!;

        // ✅ Dùng delegate để đóng cửa sổ từ View
        public Action? CloseAction { get; set; }

        public PaymentViewModel(IBillingService billingService, IPrintService printService)
        {
            _billingService = billingService;
            _printService = printService;
        }

        // ✅ Command: Xử lý thanh toán
        [RelayCommand]
        private async Task ProcessPayment()
        {
            if (Invoice == null) return;

            var paymentMethod = "Tiền mặt"; // Có thể lấy từ ComboBox trong View

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
                CloseAction?.Invoke(); // ✅ Gọi đóng cửa sổ
            }
            else
            {
                MessageBox.Show("Thanh toán thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ Command: In hóa đơn (đổi tên tránh trùng)
        [RelayCommand]
        private async Task PrintInvoiceCommand()
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
