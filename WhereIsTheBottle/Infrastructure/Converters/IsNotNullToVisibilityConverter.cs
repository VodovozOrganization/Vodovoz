using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WhereIsTheBottle.Infrastructure
{
	public class IsNotNullToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value == null ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is Visibility))
			{
				throw new ArgumentException(nameof(value));
			}

			return (Visibility)value == Visibility.Visible;
		}
	}
}