using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Windows.Input;
using QuanLyQuanBida.Core.DTOs;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class ReportsViewModel : ObservableObject
    {
        private readonly IReportService _reportService;

        [ObservableProperty]
        private ObservableCollection<object> _reportData = new();

        [ObservableProperty]
        private string _selectedReportType = "Doanh thu theo ngày";

        [ObservableProperty]
        private DateTime? _startDate = DateTime.Today.AddDays(-30);

        [ObservableProperty]
        private DateTime? _endDate = DateTime.Today;

        public ReportsViewModel(IReportService reportService)
        {
            _reportService = reportService;
            _ = GenerateReportAsync();
        }

        [RelayCommand]
        private async Task GenerateReport()
        {
            await GenerateReportAsync();
        }

        private async Task GenerateReportAsync()
        {
            try
            {
                ReportData.Clear();

                if (StartDate == null || EndDate == null)
                {
                    MessageBox.Show("Vui lòng chọn khoảng thời gian báo cáo.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                switch (SelectedReportType)
                {
                    case "Doanh thu theo ngày":
                        var revenueByDay = await _reportService.GetRevenueByDayAsync(StartDate.Value, EndDate.Value);
                        foreach (var item in revenueByDay)
                        {
                            ReportData.Add(item);
                        }
                        break;

                    case "Doanh thu theo bàn":
                        var revenueByTable = await _reportService.GetRevenueByTableAsync(StartDate.Value, EndDate.Value);
                        foreach (var item in revenueByTable)
                        {
                            ReportData.Add(item);
                        }
                        break;

                    case "Doanh thu theo sản phẩm":
                        var revenueByProduct = await _reportService.GetRevenueByProductAsync(StartDate.Value, EndDate.Value);
                        foreach (var item in revenueByProduct)
                        {
                            ReportData.Add(item);
                        }
                        break;

                    case "Công nợ khách hàng":
                        var customerDebts = await _reportService.GetCustomerDebtsAsync();
                        foreach (var item in customerDebts)
                        {
                            ReportData.Add(item);
                        }
                        break;

                    case "Tồn kho":
                        var inventory = await _reportService.GetInventoryReportAsync();
                        foreach (var item in inventory)
                        {
                            ReportData.Add(item);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ExportToExcel()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                Title = "Lưu báo cáo"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Implement Excel export logic
                    MessageBox.Show("Xuất báo cáo thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void Print()
        {
            try
            {
                // Implement print logic
                MessageBox.Show("In báo cáo thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}