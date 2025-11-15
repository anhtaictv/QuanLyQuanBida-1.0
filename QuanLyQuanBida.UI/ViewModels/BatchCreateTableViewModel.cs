using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QuanLyQuanBida.Core.DTOs;
using QuanLyQuanBida.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using static QuanLyQuanBida.Core.DTOs.TableBatchCreateDto;
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class BatchCreateTableViewModel : ObservableObject
    {
        private readonly ITableService _tableService;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private TableBatchCreateDto _form = new();

        public Action? CloseAction { get; set; } // Để đóng cửa sổ

        public BatchCreateTableViewModel(ITableService tableService, IAuditService auditService, ICurrentUserService currentUserService, IMessenger messenger)
        {
            _tableService = tableService;
            _auditService = auditService;
            _currentUserService = currentUserService;
            _messenger = messenger; // <-- THÊM
        }

        [RelayCommand]
        private async Task Save()
        {
            if (Form.StartNumber > Form.EndNumber || Form.StartNumber <= 0)
            {
                MessageBox.Show("Số bắt đầu/kết thúc không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var tablesToCreate = new List<TableDto>();
            string paddingFormat = $"D{Form.Padding}"; // Vd: "D2" -> "01", "D3" -> "001"

            for (int i = Form.StartNumber; i <= Form.EndNumber; i++)
            {
                string numberStr = i.ToString(paddingFormat);
                tablesToCreate.Add(new TableDto
                {
                    Code = $"{Form.Prefix}{numberStr}",
                    Name = $"{Form.NamePrefix}{numberStr}",
                    Zone = Form.Zone,
                    Seats = Form.Seats
                });
            }

            try
            {
                bool success = await _tableService.CreateTablesAsync(tablesToCreate);
                if (success)
                {
                    // Ghi log 1 lần
                    string details = $"Tạo hàng loạt bàn từ {Form.Prefix}{Form.StartNumber} đến {Form.Prefix}{Form.EndNumber} tại khu vực {Form.Zone}";
                    await _auditService.LogActionAsync(_currentUserService.CurrentUser?.Id ?? 0, "CREATE_TABLE_BATCH", "Tables", null, newValue: details);

                    MessageBox.Show($"Tạo thành công {tablesToCreate.Count} bàn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    _messenger.Send(new TablesChangedMessage());
                    CloseAction?.Invoke();
                }
                else
                {
                    MessageBox.Show("Tạo bàn thất bại. Lỗi CSDL.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nghiêm trọng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseAction?.Invoke();
        }
    }
}