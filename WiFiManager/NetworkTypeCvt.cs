using System;
using System.Globalization;
using Xamarin.Forms;
using WiFiManager.Common;

namespace WiFiManager
{
    public class NetworkTypeCvt: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToString (value ).Contains("[WEP]") ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
