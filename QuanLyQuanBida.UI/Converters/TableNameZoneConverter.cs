using System;
using System.Globalization;
using System.Windows.Data;

namespace QuanLyQuanBida.UI.Converters
{
    public class TableNameZoneConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is string name && values[1] is string zone)
            {
                if (string.IsNullOrEmpty(zone))
                    return name;
                return $"{name} - {zone}";
            }
            return values[0]; 
        }

        public object[] ConvertBack(object value, Type targetType, object[] parameterTypes, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}