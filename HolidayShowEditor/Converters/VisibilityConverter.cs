using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HolidayShowEditor.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var boolVal = false;

            if(value is int || value is short || value is long || value is byte || value is decimal)
            {
                boolVal = (decimal.Parse(value.ToString())) != 0;
            }else if(value is string)
            {
                boolVal = !String.IsNullOrWhiteSpace(value.ToString());
            }
            
            else if(value is bool)
            {
                boolVal = (bool)value;
            }

            var visibility = (bool)boolVal;
            return visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return (visibility == Visibility.Visible);
        }
    }  
}