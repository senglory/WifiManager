using System;
using System.Globalization;
using Xamarin.Forms;
using WiFiManager.Common;

namespace WiFiManager
{
    /// <summary>
    /// https://stackoverflow.com/questions/37212114/datatrigger-when-greater-than-a-number/37234876
    /// </summary>
    public class EmptyStringCvt : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty (System.Convert.ToString(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
