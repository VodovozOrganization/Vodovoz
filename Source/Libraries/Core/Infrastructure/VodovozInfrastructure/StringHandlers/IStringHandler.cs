namespace VodovozInfrastructure.StringHandlers
{
	public interface IStringHandler
	{
		/// <summary>
		/// Конвертирует массив символов в числовой формат в виде строки
		/// Все символы, кроме чисел и запятой будут убраны из входящих значений
		/// Также отслеживается количество символов в дробной части
		/// </summary>
		/// <param name="chars">входной массив символов</param>
		/// <param name="fractionalPart">дробная часть</param>
		/// <returns>числовая строка, может содержать разделитель дробной части в виде запятой </returns>
		string ConvertCharsArrayToNumericString(char[] chars, int fractionalPart = 0);
	}
}
