using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyQuanBida.Core.Interfaces;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace QuanLyQuanBida.UI.ViewModels
{
    public partial class BackupViewModel : ObservableObject
    {
        private readonly IBackupService _backupService;

        // Đảm bảo các thuộc tính này được khai báo đúng
        [ObservableProperty]
        private string _backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "QuanLyBidaBackups");

        [ObservableProperty]
        private string _backupFile = string.Empty;

        public BackupViewModel(IBackupService backupService)
        {
            _backupService = backupService;

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(BackupPath))
            {
                Directory.CreateDirectory(BackupPath);
            }
        }

        [RelayCommand]
        private void BrowseBackupPath()
        {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = BackupPath;

            if (dialog.ShowDialog() == DialogResult.OK) // Sửa DialogResult
            {
                BackupPath = dialog.SelectedPath;
            }
        }

        [RelayCommand]
        private void BrowseBackupFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn tệp sao lưu",
                Filter = "Backup files (*.bak)|*.bak|All files (*.*)|*.*",
                InitialDirectory = BackupPath
            };

            if (dialog.ShowDialog() == true)
            {
                BackupFile = dialog.FileName;
            }
        }

        [RelayCommand]
        private async Task Backup()
        {
            try
            {
                var success = await _backupService.BackupDatabaseAsync(BackupPath);

                if (success)
                {
                    MessageBox.Show("Sao lưu thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Sao lưu thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sao lưu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Restore()
        {
            if (string.IsNullOrEmpty(BackupFile))
            {
                MessageBox.Show("Vui lòng chọn tệp sao lưu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Phục hồi dữ liệu sẽ thay thế toàn bộ dữ liệu hiện tại. Bạn có chắc chắn muốn tiếp tục?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _backupService.RestoreDatabaseAsync(BackupFile);

                    if (success)
                    {
                        MessageBox.Show("Phục hồi thành công. Vui lòng khởi động lại ứng dụng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        System.Windows.Application.Current.Shutdown();
                    }
                    else
                    {
                        MessageBox.Show("Phục hồi thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi phục hồi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            // Lấy cửa sổ hiện tại và đóng nó
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window.DataContext is BackupViewModel)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}