using Gamma.Binding;
using System;
using System.Globalization;

namespace Vodovoz.Infrastructure.Converters
{
	public class DoubleToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null ? value.ToString() : "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(string.IsNullOrWhiteSpace(value as string))
			{
				return null;
			}

			if(targetType == typeof(double) && double.TryParse(value.ToString(), out double number))
			{
				return number;
			}

			return null;
		}
	}
}
