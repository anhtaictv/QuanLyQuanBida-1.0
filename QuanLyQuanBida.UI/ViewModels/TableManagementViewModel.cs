using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Entities;
using QuanLyQuanBida.Core.Interfaces;
using QuanLyQuanBida.UI.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using static QuanLyQuanBida.Core.DTOs.TableBatchCreateDto;
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class TableManagementViewModel : ObservableObject, IRecipient<TablesChangedMessage>
    {
        private readonly ITableService _tableService;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private ObservableCollection<Table> _tables = new();

        [ObservableProperty]
        private TableDto _tableForm = new();

        [ObservableProperty]
        private Table? _selectedTable;

        [ObservableProperty]
        private bool _isEditing = false;

        public TableManagementViewModel(ITableService tableService, IAuditService auditService, ICurrentUserService currentUserService, IMessenger messenger) // <-- THÊM `IMessenger messenger` VÀO ĐÂY
        {
            _tableService = tableService;
            _auditService = auditService;
            _currentUserService = currentUserService;
            _messenger = messenger; // <-- DÒNG 40 SẼ HẾT BÁO LỖI
            _messenger.RegisterAll(this);
            _ = LoadTablesAsync();
        }

        private async Task LoadTablesAsync()
        {
            Tables.Clear();
            var list = await _tableService.GetAllTablesAsync();
            foreach (var t in list)
            {
                Tables.Add(t);
            }
        }

        [RelayCommand]
        private void AddNew()
        {
            SelectedTable = null;
            TableForm = new TableDto { Status = "Free", Seats = 4 };
            IsEditing = false;
        }

        [RelayCommand]
        private void EditTable(Table table)
        {
            if (table == null) return;
            SelectedTable = table;
            TableForm = new TableDto
            {
                Id = table.Id,
                Code = table.Code,
                Name = table.Name,
                Zone = table.Zone,
                Seats = table.Seats,
                Status = table.Status,
                Note = table.Note
            };
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SaveTable()
        {
            if (string.IsNullOrWhiteSpace(TableForm.Name) || string.IsNullOrWhiteSpace(TableForm.Code))
            {
                MessageBox.Show("Vui lòng điền Mã bàn và Tên bàn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var currentUserId = _currentUserService.CurrentUser?.Id ?? 0;
            string logAction = "CREATE_TABLE";
            string? oldValue = null;
            int? targetId = null;

            try
            {
                if (IsEditing)
                {
                    // Cập nhật thông tin cho Audit Log
                    logAction = "UPDATE_TABLE";
                    targetId = TableForm.Id;
                    var existing = await _tableService.GetTableByIdAsync(TableForm.Id);
                    if (existing != null)
                    {
                        oldValue = $"Code: {existing.Code}, Name: {existing.Name}, Zone: {existing.Zone}";
                    }

                    await _tableService.UpdateTableAsync(TableForm);
                }
                else
                {
                    var newTable = await _tableService.CreateTableAsync(TableForm);
                    targetId = newTable.Id; // Lấy ID sau khi tạo
                }

                // Ghi Log sau khi thành công
                await _auditService.LogActionAsync(currentUserId, logAction, "Tables", targetId,
                    oldValue,
                    newValue: $"Code: {TableForm.Code}, Name: {TableForm.Name}, Zone: {TableForm.Zone}");

                MessageBox.Show("Lưu thông tin bàn thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                _messenger.Send(new TablesChangedMessage());
                AddNew();
            }
            catch (Exception ex)
            {
                // Báo lỗi chi tiết hơn
                MessageBox.Show($"Lỗi khi lưu bàn: {ex.Message}\n\nInner Exception: {ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteTable(Table table)
        {
            if (table == null || MessageBox.Show($"Bạn có chắc muốn xóa '{table.Name}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try // Thêm try-catch
            {
                var success = await _tableService.DeleteTableAsync(table.Id);
                if (success)
                {
                    // Ghi Log
                    await _auditService.LogActionAsync(_currentUserService.CurrentUser?.Id ?? 0, "DELETE_TABLE", "Tables", table.Id,
                        oldValue: $"Code: {table.Code}, Name: {table.Name}");

                    MessageBox.Show("Xóa bàn thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    _messenger.Send(new TablesChangedMessage());
                }
                else
                {
                    MessageBox.Show("Xóa bàn thất bại. Bàn có thể đang được sử dụng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex) // Bắt lỗi CSDL (nếu có)
            {
                MessageBox.Show($"Lỗi khi xóa bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ShowBatchCreateWindow()
        {
            // Mở cửa sổ mới
            var batchView = App.Services.GetRequiredService<BatchCreateTableWindow>();
            batchView.ShowDialog();

            // Sau khi cửa sổ đóng, tải lại danh sách bàn
            _ = LoadTablesAsync();
        }
        public async void Receive(TablesChangedMessage message)
        {
            // Khi nhận được tin nhắn, tải lại danh sách bàn
            await LoadTablesAsync();
        }
    }
}