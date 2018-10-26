using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using WiFiManager.Common;
using System.Globalization;
using WiFiManager.Common.BusinessObjects;
using System.Collections.ObjectModel;

namespace WiFiManager
{
    class SafeGetCoordsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var castedVal = (ObservableCollection<CoordsAndPower>)value;
            if (castedVal.Count > 0)
                return castedVal[castedVal.Count - 1];
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
