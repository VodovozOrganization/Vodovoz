using System;
using System.Globalization;
using System.Windows.Data;

namespace WhereIsTheBottle.Infrastructure
{
	public class InverseBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}
			if(!(value is bool))
			{
				throw new InvalidOperationException("The value must be a boolean");
			}

			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}