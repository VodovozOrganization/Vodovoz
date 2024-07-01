namespace VodovozInfrastructure.StringHandlers
{
	public class StringHandler : IStringHandler
	{
		/// <summary>
		/// Конвертирует массив символов в числовой формат в виде строки
		/// Все символы, кроме чисел будут убраны из входящих значений
		/// Также отслеживается количество символов в дробной части
		/// и заменяется разделитель в зависимости от входящего параметра isCommaFract
		/// По умолчанию разделитель запятая
		/// </summary>
		/// <param name="chars">входной массив символов</param>
		/// <param name="fractionalPart">дробная часть</param>
		/// <param name="isCommaSeparator">true - разделитель запятая, false - разделитель точка</param>
		/// <returns>числовая строка, может содержать разделитель дробной части</returns>
		public string ConvertCharsArrayToNumericString(char[] chars, int fractionalPart = 0, bool isCommaSeparator = true)
		{
			var result = string.Empty;
			var hasDotOrComma = false;

			foreach(var ch in chars)
			{
				var chValue = (uint)ch;

				//Если точка
				if(chValue == 46 && !hasDotOrComma && fractionalPart > 0)
				{
					AddCommaOrDotSeparator(ref result, isCommaSeparator);
					hasDotOrComma = true;
					continue;
				}

				//Если запятая
				if(chValue == 44 && !hasDotOrComma && fractionalPart > 0)
				{
					AddCommaOrDotSeparator(ref result, isCommaSeparator);
					hasDotOrComma = true;
					continue;
				}

				//Если числа от 0 до 9
				if(chValue > 47 && chValue < 58)
				{
					if(hasDotOrComma)
					{
						if(fractionalPart > 0)
						{
							result += ch;
							fractionalPart--;
						}
						
						continue;
					}

					result += ch;
				}
			}

			return result;
		}

		private void AddCommaOrDotSeparator(ref string result, bool isCommaSeparator)
		{
			if(isCommaSeparator)
			{
				result += ',';
			}
			else
			{
				result += '.';
			}
		}
	}
}
