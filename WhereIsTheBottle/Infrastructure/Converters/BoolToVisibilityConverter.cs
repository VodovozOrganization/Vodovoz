using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WhereIsTheBottle.Infrastructure
{
	public class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is not bool)
			{
				throw new ArgumentException(nameof(value));
			}
			return (bool)value ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is not Visibility)
			{
				throw new ArgumentException(nameof(value));
			}
			return (Visibility)value == Visibility.Visible;
		}
	}
}