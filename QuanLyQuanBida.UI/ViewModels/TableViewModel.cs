using CommunityToolkit.Mvvm.ComponentModel;
using QuanLyQuanBida.Core.Entities;
using System;

namespace QuanLyQuanBida.UI.ViewModels
{
    // Class này "bọc" (wraps) class Table gốc để thêm logic hiển thị
    public partial class TableViewModel : ObservableObject
    {
        public Table Table { get; }

        [ObservableProperty]
        private Session? _currentSession;

        [ObservableProperty]
        private string _elapsedTime = "00:00:00"; // Thời gian chơi

        public string Name => Table.Name;
        public string Status => CurrentSession != null ? "Occupied" : Table.Status; // Tự động đổi "Occupied"
        public string Zone => Table.Zone ?? "Khu vực chung";
        public bool IsSessionActive => CurrentSession != null;

        public TableViewModel(Table table)
        {
            Table = table;
        }

        // Cập nhật đồng hồ
        public void UpdateElapsedTime()
        {
            if (CurrentSession != null)
            {
                var duration = DateTime.UtcNow - CurrentSession.StartAt;
                // Xử lý cả trường hợp tạm dừng (nếu có)
                if (CurrentSession.PauseAt.HasValue && CurrentSession.ResumeAt.HasValue)
                {
                    duration -= (CurrentSession.ResumeAt.Value - CurrentSession.PauseAt.Value);
                }
                else if (CurrentSession.PauseAt.HasValue && !CurrentSession.ResumeAt.HasValue)
                {
                    // Nếu đang tạm dừng
                    duration = CurrentSession.PauseAt.Value - CurrentSession.StartAt;
                }

                ElapsedTime = duration.ToString(@"hh\:mm\:ss");
            }
            else
            {
                ElapsedTime = "00:00:00";
            }
        }
    }
}