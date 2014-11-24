using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace HolidayShowEditor.Converters
{
    public abstract class EnumKeyValueDictionaryBase<T, T1> : IValueConverter 
                                                where T : struct, IConvertible 
                                                where T1 : struct
    {
        public static SortedDictionary<long, string> InternalValues { get; set; }

        public SortedDictionary<long, string> AvailableValues
        {
            get { return InternalValues; }
        }

        static EnumKeyValueDictionaryBase()
        {
            InternalValues = new SortedDictionary<long, string>();
            foreach (var eventType in Enum.GetValues(typeof(T)))
            {
                InternalValues.Add(long.Parse(((T1)eventType).ToString()), string.Format("{0}", eventType));
            }
        }


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            var v = long.Parse(value.ToString());

            if(InternalValues.ContainsKey(v))
            {
                return (from x in InternalValues where x.Key == v select x).FirstOrDefault();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
