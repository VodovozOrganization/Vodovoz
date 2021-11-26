using System;
using System.Globalization;
using System.Windows.Data;

namespace WhereIsTheBottle.Infrastructure
{
	public class ZeroToEmptyStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}
			var type = value.GetType();
			dynamic defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
			dynamic typedValue = System.Convert.ChangeType(value, type);
			return defaultValue == typedValue ? "" : value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}