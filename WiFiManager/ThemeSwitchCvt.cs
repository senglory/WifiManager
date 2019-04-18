using System;
using System.Globalization;

using Xamarin.Forms;



namespace WiFiManager
{
    public class ThemeSwitchCvt : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToBoolean (value) ? "Black" : "White";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
