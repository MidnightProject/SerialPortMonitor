using System;
using System.Globalization;
using System.Windows.Data;

namespace SerialPortMonitor.Converter
{
    public class OpenToToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return String.Empty;
            }

            if ((Boolean)value == true)
            {
                return "The port is open.";
            }
            else
            {
                return "The port is closed.";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
