using System;
using System.Globalization;
using Xamarin.Forms;
using WiFiManager.Common;

namespace WiFiManager
{
    public class NetworkColorSelector: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(System.Convert.ToString(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
