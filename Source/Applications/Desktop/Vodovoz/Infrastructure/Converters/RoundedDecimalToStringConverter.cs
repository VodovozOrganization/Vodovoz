using System;
using System.Globalization;
using Gamma.Binding;

namespace Vodovoz.Infrastructure.Converters
{
	public class RoundedDecimalToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return double.TryParse(value?.ToString(), out double res) ? res : 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(string.IsNullOrWhiteSpace(value as string))
			{
				return default(decimal);
			}

			var stringValue = (string)value;

			// Первая попытка парсинга
			if(decimal.TryParse(stringValue, out decimal result))
			{
				return Math.Round(result, MidpointRounding.AwayFromZero);
			}

			// Вторая попытка: заменяем точку на запятую
			var modifiedValue = stringValue.Replace('.', ',');
			if(decimal.TryParse(modifiedValue, out result))
			{
				return Math.Round(result, MidpointRounding.AwayFromZero);
			}

			// Третья попытка: заменяем запятую на точку
			modifiedValue = stringValue.Replace(',', '.');
			if(decimal.TryParse(modifiedValue, out result))
			{
				return Math.Round(result, MidpointRounding.AwayFromZero);
			}

			// Если все попытки неудачны
			return default(decimal);
		}
	}
}
