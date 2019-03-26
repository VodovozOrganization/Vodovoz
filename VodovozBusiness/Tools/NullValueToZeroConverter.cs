using System;
using System.Globalization;
using Gamma.Binding;

namespace Vodovoz.Tools
{
	public class NullValueToZeroConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return double.TryParse(value?.ToString(), out double res) ? res : 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return int.TryParse(value?.ToString(), out int res) ? res : 0;
		}
	}
}
