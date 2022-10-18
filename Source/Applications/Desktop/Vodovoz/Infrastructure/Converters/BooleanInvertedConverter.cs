using System;
using System.Globalization;
using Gamma.Binding;
namespace Vodovoz.Infrastructure.Converters
{
	public class BooleanInvertedConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}
	}
}
