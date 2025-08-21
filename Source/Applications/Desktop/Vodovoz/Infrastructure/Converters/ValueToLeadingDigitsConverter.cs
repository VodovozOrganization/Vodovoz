using Gamma.Binding;
using System;
using System.Globalization;
using System.Linq;

namespace Vodovoz.Infrastructure.Converters
{
	/// <summary>
	/// Конвертер значений в ведущие цифры(слева)
	/// </summary>
	public class ValueToLeadingDigitsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return double.TryParse(value?.ToString(), out double res) ? res : 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value == null)
			{
				return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
			}

			var stringValue = value.ToString();

			var numericPart = new string(stringValue.TakeWhile(char.IsDigit).ToArray());

			if(string.IsNullOrEmpty(numericPart))
			{
				return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
			}

			try
			{
				var numericValue = System.Convert.ToDouble(numericPart, culture);
				return System.Convert.ChangeType(numericValue, targetType, culture);
			}
			catch
			{
				return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
			}
		}
	}
}
