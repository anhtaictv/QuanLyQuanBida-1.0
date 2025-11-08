using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class ShiftManagementViewModel : ObservableObject
    {
        private readonly IShiftService _shiftService;
        private readonly ICurrentUserService _currentUserService;

        [ObservableProperty]
        private ObservableCollection<Shift> _shifts = new();

        [ObservableProperty]
        private DateTime? _startDate = DateTime.Today.AddDays(-30);

        [ObservableProperty]
        private DateTime? _endDate = DateTime.Today;

        [ObservableProperty]
        private Shift? _currentShift;

        public ShiftManagementViewModel(
            IShiftService shiftService,
            ICurrentUserService currentUserService)
        {
            _shiftService = shiftService;
            _currentUserService = currentUserService;

            _ = LoadCurrentShiftAsync();
            _ = LoadShiftHistoryAsync();
        }

        private async Task LoadCurrentShiftAsync()
        {
            if (_currentUserService.CurrentUser == null) return;

            CurrentShift = await _shiftService.GetCurrentShiftAsync(_currentUserService.CurrentUser.Id);
        }

        private async Task LoadShiftHistoryAsync()
        {
            if (_currentUserService.CurrentUser == null) return;

            Shifts.Clear();

            var shiftsFromDb = await _shiftService.GetShiftHistoryAsync(
                _currentUserService.CurrentUser.Id,
                StartDate,
                EndDate);

            foreach (var shift in shiftsFromDb)
            {
                Shifts.Add(shift);
            }
        }

        [RelayCommand]
        private void OpenShift()
        {
            var openShiftWindow = new OpenShiftWindow();
            if (openShiftWindow.ShowDialog() == true)
            {
                _ = LoadCurrentShiftAsync();
            }
        }

        [RelayCommand]
        private void CloseShift()
        {
            if (CurrentShift == null)
            {
                MessageBox.Show("Không có ca làm việc nào đang mở.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var closeShiftWindow = new CloseShiftWindow(CurrentShift.Id);
            if (closeShiftWindow.ShowDialog() == true)
            {
                _ = LoadCurrentShiftAsync();
                _ = LoadShiftHistoryAsync();
            }
        }

        [RelayCommand]
        private async Task LoadShiftHistory()
        {
            await LoadShiftHistoryAsync();
        }
    }
}