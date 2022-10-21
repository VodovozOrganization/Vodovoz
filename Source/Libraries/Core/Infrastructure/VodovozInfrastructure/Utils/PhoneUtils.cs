using System;
using System.Text.RegularExpressions;

namespace VodovozInfrastructure.Utils
{
	public class PhoneUtils
	{
	
		 /// <summary>
		 /// Removes the non digit.
		 /// </summary>
		 /// <returns>The non digit.</returns>
		 /// <param name="s">String with digits.</param>
		public static string RemoveNonDigit(string s)
		{
			return Regex.Replace(s, "[^.0-9]", "");
		}
	
		/// <summary>
		/// Возвращает обработанную строку и true если номер нужно поискать и до и после обработки.
		/// </summary>
		/// <returns>Возвращает только цифры номера без +7/8/()/- .</returns>
		/// <param name="number">Номер телефона.</param>
		/// <param name="needSearchBoth">Если <c>true</c> нужно поискать и до и после обработки.</param>
		public static string NumberTrim(string number, out bool needSearchBoth)
		{
			var temp = RemoveNonDigit(number);
			needSearchBoth = false;

			// Console.WriteLine(temp);
			if(temp.StartsWith("7", StringComparison.Ordinal)) // без учета региональных настроек
			{
				needSearchBoth = false;
				return temp.Substring(1);
			} 
			else if(temp.StartsWith("8", StringComparison.Ordinal)) 
			{
				needSearchBoth = true;
				return temp.Substring(1);
			}

			return temp;
		}

	}
}
