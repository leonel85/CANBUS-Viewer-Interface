using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CANBUSViewerInterface.Converters
{
    public class IntToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format("{0:X" + parameter.ToString() + "}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (int.Parse(parameter.ToString()) > 2)
                return int.Parse(value.ToString(), System.Globalization.NumberStyles.HexNumber);
            else
                return byte.Parse(value.ToString(), System.Globalization.NumberStyles.HexNumber);
        }
    }
}
