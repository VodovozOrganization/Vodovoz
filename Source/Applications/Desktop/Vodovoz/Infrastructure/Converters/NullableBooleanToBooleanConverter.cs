using System;
using System.Globalization;
using Gamma.Binding;

namespace Vodovoz.Infrastructure.Converters
{
	public class NullableBooleanToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is bool?)) {
				return false;
			}
			return ((bool?)value).HasValue && ((bool?)value).Value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is bool)) {
				return null;
			}
			return ((bool)value) == false ? null : (bool?)true;
		}
	}
}
