using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System.Windows;
using static Azure.Core.HttpHeader;
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class ShiftManagementViewModel : ObservableObject
    {
        private readonly IShiftService _shiftService;
        private readonly ICurrentUserService _currentUserService;

        [ObservableProperty]
        private Shift? _currentShift;

        [ObservableProperty]
        private bool _isShiftOpen = false;

        [ObservableProperty]
        private decimal _openingCash;

        [ObservableProperty]
        private decimal _closingCash;

        [ObservableProperty]
        private string _notes = string.Empty;

        // TODO: Cần 1 service để tính toán doanh thu trong ca
        [ObservableProperty]
        private decimal _cashRevenueInShift = 0;

        public ShiftManagementViewModel(IShiftService shiftService, ICurrentUserService currentUserService)
        {
            _shiftService = shiftService;
            _currentUserService = currentUserService;
            _ = LoadActiveShiftAsync();
        }

        private async Task LoadActiveShiftAsync()
        {
            if (_currentUserService.CurrentUser == null) return;

            CurrentShift = await _shiftService.GetActiveShiftByUserIdAsync(_currentUserService.CurrentUser.Id);
            IsShiftOpen = (CurrentShift != null);

            if (IsShiftOpen)
            {
                OpeningCash = CurrentShift.OpeningCash;
                // TODO: Load revenue calculation here
            }
        }

        [RelayCommand]
        private async Task OpenShift()
        {
            if (_currentUserService.CurrentUser == null) return;

            var newShift = await _shiftService.OpenShiftAsync(_currentUserService.CurrentUser.Id, OpeningCash);
            if (newShift != null)
            {
                CurrentShift = newShift;
                IsShiftOpen = true;
                MessageBox.Show("Mở ca thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Không thể mở ca. Có thể đã có ca đang hoạt động.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task CloseShift()
        {
            if (CurrentShift == null) return;

            if (MessageBox.Show("Bạn có chắc chắn muốn đóng ca làm việc này?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            var closedShift = await _shiftService.CloseShiftAsync(CurrentShift.Id, ClosingCash, Notes);
            if (closedShift != null)
            {
                IsShiftOpen = false;
                CurrentShift = null;
                OpeningCash = 0;
                ClosingCash = 0;
                Notes = string.Empty;
                MessageBox.Show("Đóng ca thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}