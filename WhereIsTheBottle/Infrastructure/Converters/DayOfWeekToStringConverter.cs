using System;
using System.Globalization;
using System.Windows.Data;

namespace WhereIsTheBottle.Infrastructure
{
	public class DayOfWeekToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is not DayOfWeek dayOfWeek)
			{
				throw new ArgumentException("Invalid value argument", nameof(value));
			}

			return dayOfWeek switch
			{
				DayOfWeek.Monday => "пн",
				DayOfWeek.Tuesday => "вт",
				DayOfWeek.Wednesday => "ср",
				DayOfWeek.Thursday => "чт",
				DayOfWeek.Friday => "пт",
				DayOfWeek.Saturday => "сб",
				DayOfWeek.Sunday => "вс",
				_ => throw new ArgumentOutOfRangeException(nameof(value), dayOfWeek, null)
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
