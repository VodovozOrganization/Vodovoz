using System;
using System.Globalization;
using Gamma.Binding;

namespace Vodovoz.Infrastructure.Converters
{
	public class DecimalToDoubleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return decimal.ToDouble((decimal)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
		}
	}
}
