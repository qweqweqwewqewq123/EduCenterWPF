using System;
using System.Globalization;
using System.Windows.Data;

namespace EduCenterWPF
{
    public class WidthMinusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width && parameter is string paramStr && double.TryParse(paramStr, out double subtract))
            {
                return Math.Max(0, width - subtract);
            }
            
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 