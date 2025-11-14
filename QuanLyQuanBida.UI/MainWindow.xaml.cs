using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanBida.UI.ViewModels;
using System.Windows;
// SỬA: Xóa using System.Windows.Input và các sự kiện click

namespace QuanLyQuanBida.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext được gán tự động từ App.Services
            this.DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
        }

        // SỬA: Xóa toàn bộ sự kiện 'Border_MouseLeftButtonDown'
    }

    // SỬA: Thêm Converter cho màu sắc (bạn có thể tạo file riêng)
    public class TableStatusToBrushConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value?.ToString()) switch
            {
                "Occupied" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightCoral),
                "Reserved" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue),
                "Maintenance" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray),
                _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGreen), // "Free"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // SỬA: Thêm Converter để ẩn/hiện panel
    public class NullToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}