using System;
using System.Globalization;
using Xamarin.Forms;

namespace WiFiManager
{
    /// <summary>
    /// https://stackoverflow.com/questions/37212114/datatrigger-when-greater-than-a-number/37234876
    /// </summary>
    public class GreaterThan : IValueConverter
    {
        public GreaterThan()
        {
        }
        //  The only public constructor is one that requires a double argument.
        //  Because of that, the XAML editor will put a blue squiggly on it if 
        //  the argument is missing in the XAML. 
        //public GreaterThan(double opnd)
        //{
        //    Operand = opnd;
        //}

        /// <summary>
        /// Converter returns true if value is greater than this.
        /// 
        /// Don't let this be public, because it's required to be initialized 
        /// via the constructor. 
        /// </summary>
        protected decimal Operand { get; set; }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Math.Abs(System.Convert.ToDecimal( value)) > Operand;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
