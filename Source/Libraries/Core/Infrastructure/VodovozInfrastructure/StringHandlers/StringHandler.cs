namespace VodovozInfrastructure.StringHandlers
{
	public class StringHandler : IStringHandler
	{
		/// <summary>
		/// Конвертирует массив символов в числовой формат в виде строки
		/// Все символы, кроме чисел и запятой будут убраны из входящих значений
		/// Также отслеживается количество символов в дробной части
		/// </summary>
		/// <param name="chars">входной массив символов</param>
		/// <param name="fractionalPart">дробная часть</param>
		/// <returns>числовая строка, может содержать разделитель дробной части в виде запятой</returns>
		public string ConvertCharsArrayToNumericString(char[] chars, int fractionalPart = 0)
		{
			var result = string.Empty;
			var hasDotOrComma = false;

			foreach(var ch in chars)
			{
				var chValue = (uint)ch;

				//Если точка
				if(chValue == 46 && !hasDotOrComma && fractionalPart > 0)
				{
					result += ',';
					hasDotOrComma = true;
					continue;
				}

				//Если запятая
				if(chValue == 44 && !hasDotOrComma && fractionalPart > 0)
				{
					result += ch;
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
	}
}
