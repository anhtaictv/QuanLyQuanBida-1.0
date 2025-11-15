using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QuanLyQuanBida.UI.Converters
{
    public class TableStatusToBrushConverter : IValueConverter
    {
        // SỬA: Dùng màu đẹp hơn
        private readonly SolidColorBrush _occupiedBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF0F0")); // Đỏ nhạt
        private readonly SolidColorBrush _freeBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F0FFF0")); // Xanh nhạt
        private readonly SolidColorBrush _maintenanceBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F5F5F5")); // Xám nhạt
        private readonly SolidColorBrush _reservedBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F0F8FF")); // Xanh dương nhạt

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value?.ToString()) switch
            {
                "Occupied" => _occupiedBrush,
                "Reserved" => _reservedBrush,
                "Maintenance" => _maintenanceBrush,
                _ => _freeBrush, // "Free"
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}