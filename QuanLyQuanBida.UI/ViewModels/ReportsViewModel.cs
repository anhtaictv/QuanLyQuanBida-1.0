using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Windows.Input;
using QuanLyQuanBida.Core.DTOs;
using System;
using System.Threading.Tasks;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class ReportsViewModel : ObservableObject
    {
        private readonly IReportService _reportService;
        private bool _isInitialized = false; 
        private bool _isLoading = false;
        private bool _isBatchUpdatingDates = false;

        [ObservableProperty]
        private ObservableCollection<object> _reportData = new();
        public ObservableCollection<string> AvailableReportTypes { get; } = new();

        // SỬA: Thêm partial method
        [ObservableProperty]
        private string _selectedReportType = "Báo cáo chi tiết hóa đơn";
        partial void OnSelectedReportTypeChanged(string value)
        {
            if (_isInitialized) _ = GenerateReportAsync();
        }

        // SỬA: Thêm partial method
        [ObservableProperty]
        private DateTime? _startDate;
        partial void OnStartDateChanged(DateTime? value)
        {
            if (_isInitialized && !_isBatchUpdatingDates) _ = GenerateReportAsync();
        }

        // SỬA: Thêm partial method
        [ObservableProperty]
        private DateTime? _endDate;
        partial void OnEndDateChanged(DateTime? value)
        {
            if (_isInitialized && !_isBatchUpdatingDates) _ = GenerateReportAsync();
        }

        public ReportsViewModel(IReportService reportService)
        {
            _reportService = reportService;

            AvailableReportTypes.Add("Báo cáo chi tiết hóa đơn");
            AvailableReportTypes.Add("Doanh thu theo ngày");
            AvailableReportTypes.Add("Doanh thu theo giờ");
            AvailableReportTypes.Add("Doanh thu theo ca (Nhân viên)");
            AvailableReportTypes.Add("Doanh thu theo bàn");
            AvailableReportTypes.Add("Doanh thu theo sản phẩm");
            AvailableReportTypes.Add("Tồn kho");
            AvailableReportTypes.Add("Công nợ khách hàng");

            SetDateToday(); 
            _ = GenerateReportAsync(); 
            _isInitialized = true;
        }

        [RelayCommand]
        private async Task GenerateReport()
        {
            await GenerateReportAsync();
        }

        private async Task GenerateReportAsync()
        {
            if (_isLoading) return; 
            _isLoading = true;

            try
            {
                ReportData.Clear();
                if (StartDate == null || EndDate == null)
                {
                    return; 
                }

                switch (SelectedReportType)
                {
                    case "Doanh thu theo ngày":
                        var revenueByDay = await _reportService.GetRevenueByDayAsync(StartDate.Value, EndDate.Value);
                        foreach (var item in revenueByDay) ReportData.Add(item);
                        break;
                    case "Báo cáo chi tiết hóa đơn":
                        var detailedInvoices = await _reportService.GetDetailedInvoiceReportAsync(StartDate.Value, EndDate.Value);
                        foreach (var item in detailedInvoices) ReportData.Add(item);
                        break;
                    case "Doanh thu theo giờ":
                        var revenueByHour = await _reportService.GetRevenueByHourAsync(StartDate.Value, EndDate.Value);
                        foreach (var item in revenueByHour) ReportData.Add(item);
                        break;
                    case "Doanh thu theo ca (Nhân viên)":
                        var revenueByEmp = await _reportService.GetRevenueByEmployeeAsync(StartDate.Value, EndDate.Value);
                        foreach (var item in revenueByEmp) ReportData.Add(item);
                        break;
                    case "Doanh thu theo bàn":
                        var revenueByTable = await _reportService.GetRevenueByTableAsync(StartDate.Value, EndDate.Value);
                        foreach (var item in revenueByTable) ReportData.Add(item);
                        break;
                    case "Doanh thu theo sản phẩm":
                        var revenueByProduct = await _reportService.GetRevenueByProductAsync(StartDate.Value, EndDate.Value);
                        foreach (var item in revenueByProduct) ReportData.Add(item);
                        break;
                    case "Công nợ khách hàng":
                        var customerDebts = await _reportService.GetCustomerDebtsAsync();
                        foreach (var item in customerDebts) ReportData.Add(item);
                        break;
                    case "Tồn kho":
                        var inventory = await _reportService.GetInventoryReportAsync();
                        foreach (var item in inventory) ReportData.Add(item);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        [RelayCommand]
        private void SetDateToday()
        {
            _isBatchUpdatingDates = true; // Báo cho hệ thống: "Bắt đầu cập nhật batch"
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1).AddTicks(-1);
            _isBatchUpdatingDates = false; // Báo: "Cập nhật xong"
            _ = GenerateReportAsync(); // Chạy báo cáo 1 lần duy nhất
        }

        [RelayCommand]
        private void SetDateThisWeek()
        {
            _isBatchUpdatingDates = true;
            // SỬA LOGIC: Bắt đầu từ T2 (Monday)
            StartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            EndDate = StartDate.Value.AddDays(7).AddTicks(-1);
            _isBatchUpdatingDates = false;
            _ = GenerateReportAsync();
        }

        [RelayCommand]
        private void SetDateThisMonth()
        {
            _isBatchUpdatingDates = true;
            StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            EndDate = StartDate.Value.AddMonths(1).AddTicks(-1);
            _isBatchUpdatingDates = false;
            _ = GenerateReportAsync();
        }

        [RelayCommand]
        private void SetDateLastMonth()
        {
            _isBatchUpdatingDates = true;
            var lastMonth = DateTime.Today.AddMonths(-1);
            StartDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
            EndDate = StartDate.Value.AddMonths(1).AddTicks(-1);
            _isBatchUpdatingDates = false;
            _ = GenerateReportAsync();
        }

        // --- CÁC HÀM XUẤT FILE (Giữ nguyên) ---
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
                    // TODO: Implement Excel export logic
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
                // TODO: Implement print logic
                MessageBox.Show("In báo cáo thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}