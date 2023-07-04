using System;
using System.Globalization;
using Gamma.Binding;

namespace Vodovoz.Infrastructure.Converters
{
	public class NullableIntToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(string.IsNullOrWhiteSpace(value as string))
			{
				return null;
			}

			if(targetType == typeof(int?) && int.TryParse(value.ToString(), out var number))
			{
				return number;
			}

			return null;
		}
	}
	
	public class NullableDecimalToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is null)
			{
				return null;
			}

			var f = string.Format(CultureInfo.InvariantCulture, "{0}", value);
			return f;
			//	var f = value?.ToString(CultureInfo.InvariantCulture);
			//return f;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(string.IsNullOrWhiteSpace(value as string))
			{
				return null;
			}
			
			
			if(targetType == typeof(decimal?) && decimal.TryParse(value.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
			{
				return number;
			}

			return null;
		}
	}
}
