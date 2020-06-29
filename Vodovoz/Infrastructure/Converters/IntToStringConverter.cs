using System;
using System.Globalization;
using Gamma.Binding;

namespace Vodovoz.Infrastructure.Converters
{
	public class IntToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(String.IsNullOrWhiteSpace(value as String))
				return null;

			int number = 0;
			if(targetType == typeof(int?) && Int32.TryParse(value.ToString(), out number))
				return number;

			return null;
		}
	}
}
