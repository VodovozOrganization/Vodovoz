using System;
using System.Globalization;
using System.Windows.Data;

namespace WhereIsTheBottle.Infrastructure
{
	public class PlusConstantConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is double doubleValue))
			{
				throw new ArgumentNullException(nameof(value));
			}

			var doublePar = System.Convert.ToDouble(parameter);
			if(doublePar == 0)
			{
				throw new InvalidOperationException(nameof(parameter));
			}

			return doubleValue + doublePar;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}