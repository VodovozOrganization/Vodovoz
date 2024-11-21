using System;
using System.Globalization;
using Gamma.Binding;

namespace Vodovoz.Infrastructure.Converters
{
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
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(string.IsNullOrWhiteSpace(value as string))
			{
				return null;
			}

			if(targetType == typeof(decimal?) && decimal.TryParse(value.ToString(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
					CultureInfo.InvariantCulture, out var number))
			{
				return number;
			}

			return null;
		}
	}
}
