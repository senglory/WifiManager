using System;
using System.Globalization;

using Xamarin.Forms;



namespace WiFiManager
{
    class ThemeSwitchCvtButtons : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToBoolean(value) ? Color.Black : Color.White;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
