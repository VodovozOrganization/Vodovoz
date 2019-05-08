using System;
using System.Globalization;
using Gamma.Binding;

namespace Vodovoz.Tools
{
	public class IntToStringValuableConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(String.IsNullOrWhiteSpace(value as String))
				return null;
				
			if(targetType == typeof(int) && Int32.TryParse(value.ToString(), out int number))
				return number;

			return null;
		}
	}
}
