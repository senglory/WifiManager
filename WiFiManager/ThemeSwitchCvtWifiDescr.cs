using System;
using System.Globalization;

using Xamarin.Forms;



namespace WiFiManager
{
	public class ThemeSwitchCvtWifiDescr : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToBoolean(value) ? Color.LightGreen : Color.Black;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
