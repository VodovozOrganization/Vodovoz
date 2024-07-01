namespace VodovozInfrastructure.StringHandlers
{
	public interface IStringHandler
	{
		/// <summary>
		/// Конвертирует массив символов в числовой формат в виде строки
		/// Все символы, кроме чисел убраны из входящих значений
		/// Также отслеживается количество символов в дробной части и заменяется разделитель в зависимости от входящего параметра isCommaFract
		/// По умолчанию разделитель запятая
		/// </summary>
		/// <param name="chars">входной массив символов</param>
		/// <param name="fractionalPart">дробная часть</param>
		/// <param name="isCommaSeparator">true - разделитель запятая, false - разделитель точка</param>
		/// <returns>числовая строка, может содержать разделитель дробной части</returns>
		string ConvertCharsArrayToNumericString(char[] chars, int fractionalPart = 0, bool isCommaSeparator = true);
	}
}
