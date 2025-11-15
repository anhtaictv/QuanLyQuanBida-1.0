using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static QuanLyQuanBida.Core.DTOs.TableBatchCreateDto;
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class MoveTableViewModel : ObservableObject
    {
        private readonly ISessionService _sessionService;
        private readonly ITableService _tableService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private ObservableCollection<Table> _freeTables = new();

        [ObservableProperty]
        private Table? _selectedTargetTable;

        [ObservableProperty]
        private string _currentTableName = string.Empty;

        private int _currentSessionId;
        private int _currentUserId;

        public Action? CloseAction { get; set; }

        public MoveTableViewModel(ISessionService sessionService, ITableService tableService, ICurrentUserService currentUserService, IMessenger messenger)
        {
            _sessionService = sessionService;
            _tableService = tableService;
            _currentUserService = currentUserService;
            _messenger = messenger;
            _currentUserId = _currentUserService.CurrentUser?.Id ?? 0;
        }

        public async Task LoadDataAsync(Session currentSession)
        {
            _currentSessionId = currentSession.Id;

            // Lấy tên bàn hiện tại
            var currentTable = await _tableService.GetTableByIdAsync(currentSession.TableId);
            CurrentTableName = currentTable?.Name ?? "Không rõ";

            // Tải danh sách bàn trống
            FreeTables.Clear();
            var allTables = await _tableService.GetAllTablesAsync();
            foreach (var table in allTables.Where(t => t.Status == "Free"))
            {
                FreeTables.Add(table);
            }
        }

        [RelayCommand]
        private async Task MoveTable()
        {
            if (SelectedTargetTable == null)
            {
                MessageBox.Show("Vui lòng chọn bàn muốn chuyển đến.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                bool success = await _sessionService.MoveSessionAsync(_currentSessionId, SelectedTargetTable.Id, _currentUserId);
                if (success)
                {
                    MessageBox.Show($"Đã chuyển phiên sang {SelectedTargetTable.Name} thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    _messenger.Send(new TablesChangedMessage()); // Gửi tin nhắn
                    CloseAction?.Invoke();
                }
                else
                {
                    MessageBox.Show("Chuyển bàn thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chuyển bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}