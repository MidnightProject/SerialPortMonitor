using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace SerialPortMonitor.Converter
{
    public class ListToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            IList collection = (IList)value;
            if (collection.Count != 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Port
    {
        public string Name { get; set; }

        public string Description { get; set; }

        private Boolean open { get; set; }
        public Boolean Open
        {
            get { return open; }
            set
            {
                if (value != open)
                {
                    open = value;
                }
            }
        }
    }
}
