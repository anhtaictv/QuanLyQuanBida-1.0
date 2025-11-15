using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QuanLyQuanBida.UI.Converters
{
    public class ReportTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value là SelectedReportType (vd: "Doanh thu theo ngày")
            // parameter là tên của Báo cáo (vd: "Doanh thu theo ngày")
            return value?.ToString() == parameter?.ToString() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}