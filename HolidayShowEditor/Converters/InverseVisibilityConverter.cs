using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HolidayShowEditor.Converters
{
    public class InverseVisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {

            var boolVal = false;

            if (value is int || value is short || value is long || value is byte)
            {
                boolVal = (long.Parse(value.ToString())) != 0;
            }
            else if (value is bool)
            {
                boolVal = (bool)value;
            }

            bool visibility = (bool)boolVal;
            return !visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return (visibility != Visibility.Visible);
        }
    }
}