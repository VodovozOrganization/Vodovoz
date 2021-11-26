using System;
using System.Globalization;
using System.Windows.Data;

namespace WhereIsTheBottle.Infrastructure
{
	/// <summary>
	///     Превращает bool в int.
	///     Если parameter = null:  При значении value = true возвращает 1, иначе 0
	///     Если parameter != null: При значении value = true возвращает значение parameter как int, иначе 0
	/// </summary>
	public class BoolToIntConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is not bool boolValue)
			{
				throw new ArgumentException("Переданный параметр не является bool", nameof(value));
			}
			if(parameter != null && parameter is not int && !Int32.TryParse(parameter.ToString(), out var _))
			{
				throw new ArgumentException("Переданный параметр не является int", nameof(parameter));
			}

			if(parameter == null)
			{
				return boolValue ? 1 : 0;
			}
			var intParameter = parameter is int i ? i : Int32.Parse(parameter.ToString()!);
			return boolValue ? intParameter : 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}