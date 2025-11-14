using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class RateSettingViewModel : ObservableObject
    {
        private readonly IRateService _rateService;

        [ObservableProperty]
        private ObservableCollection<Rate> _rates = new();

        [ObservableProperty]
        private Rate _rateForm = new();

        [ObservableProperty]
        private Rate? _selectedRate;

        [ObservableProperty]
        private bool _isEditing = false;

        public RateSettingViewModel(IRateService rateService)
        {
            _rateService = rateService;
            _ = LoadRatesAsync();
            AddNew(); // Initialize form
        }

        private async Task LoadRatesAsync()
        {
            Rates.Clear();
            var list = await _rateService.GetAllRatesAsync();
            foreach (var rate in list)
            {
                Rates.Add(rate);
            }
        }

        [RelayCommand]
        private void AddNew()
        {
            SelectedRate = null;
            RateForm = new Rate { PricePerHour = 0, IsDefault = false, IsWeekendRate = false };
            IsEditing = false;
        }

        [RelayCommand]
        private void EditRate(Rate rate)
        {
            if (rate == null) return;
            SelectedRate = rate;
            // Clone data to form to avoid direct manipulation of the list item
            RateForm = new Rate
            {
                Id = rate.Id,
                Name = rate.Name,
                PricePerHour = rate.PricePerHour,
                StartTimeWindow = rate.StartTimeWindow,
                EndTimeWindow = rate.EndTimeWindow,
                IsWeekendRate = rate.IsWeekendRate,
                IsDefault = rate.IsDefault
            };
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SaveRate()
        {
            if (string.IsNullOrWhiteSpace(RateForm.Name) || RateForm.PricePerHour <= 0)
            {
                MessageBox.Show("Vui lòng điền tên và giá giờ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (IsEditing)
                {
                    await _rateService.UpdateRateAsync(RateForm);
                }
                else
                {
                    await _rateService.CreateRateAsync(RateForm);
                }

                MessageBox.Show("Lưu cấu hình giá thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadRatesAsync();
                AddNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu cấu hình giá: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteRate(Rate rate)
        {
            if (rate == null || MessageBox.Show($"Bạn có chắc muốn xóa giá '{rate.Name}'?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            if (await _rateService.DeleteRateAsync(rate.Id))
            {
                MessageBox.Show("Xóa giá thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadRatesAsync();
                AddNew();
            }
        }
    }
}