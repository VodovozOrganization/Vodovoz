using System;
using System.Globalization;
using System.Windows.Data;

namespace WhereIsTheBottle.Infrastructure
{
	public class FormatDateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}
			if(parameter == null)
			{
				throw new ArgumentNullException(nameof(parameter));
			}

			if(!(value is DateTime))
			{
				throw new InvalidOperationException($"The value must be of type {nameof(DateTime)}");
			}
			if(!(parameter is string))
			{
				throw new InvalidOperationException($"The parameter must be of type {nameof(String)}");
			}

			return ((DateTime)value).ToString((string)parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}