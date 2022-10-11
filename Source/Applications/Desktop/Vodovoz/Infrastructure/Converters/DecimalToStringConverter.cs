using System;
using System.Globalization;
using Gamma.Binding;

namespace Vodovoz.Infrastructure.Converters
{
	public class DecimalToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null ? value.ToString() : "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(string.IsNullOrWhiteSpace(value as string)) {
				return default(decimal);
			}
			if(decimal.TryParse((string)value, out decimal result)) {
				return result;
			}
			return "";
		}
	}
}
