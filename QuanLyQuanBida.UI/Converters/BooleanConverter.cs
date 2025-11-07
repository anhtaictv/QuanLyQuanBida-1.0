using System;
using System.Globalization;
using System.Windows.Data;

namespace QuanLyQuanBida.UI.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    public class TableStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Free" => "#FF90EE90", // Light green
                    "Occupied" => "#FFFFB6C1", // Light red
                    "Reserved" => "#FFF0E68C", // Light yellow
                    "Maintenance" => "#FFD3D3D3", // Light gray
                    _ => "#FFFFFFFF" // White
                };
            }
            return "#FFFFFFFF";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}