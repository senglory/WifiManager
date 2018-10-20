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
            if (System.Convert.ToString(value).Contains("[WEP]"))
                return "WEP";
            else
                if (System.Convert.ToString(value).Contains("[WPS]"))
                return "WPS";
            else return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
