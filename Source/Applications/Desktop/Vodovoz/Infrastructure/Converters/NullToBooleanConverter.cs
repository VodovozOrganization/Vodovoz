using System;
using System.Globalization;
using Gamma.Binding;
namespace Vodovoz.Infrastructure.Converters
{
	public class NullToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
