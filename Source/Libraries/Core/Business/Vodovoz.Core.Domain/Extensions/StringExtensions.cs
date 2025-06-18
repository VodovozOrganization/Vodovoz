using System;
using System.Linq;

namespace Vodovoz.Core.Domain.Extensions
{
	public static class StringExtensions
	{
		public static string FromPascalCaseToSnakeCase(this string sentence)
		{
			var result = string.Empty;

			for(var i = 0; i < sentence.Length; i++)
			{
				if(sentence[i] == char.ToUpper(sentence[i]) && i > 0)
				{
					result += $"_";
				}

				result += char.ToLower(sentence[i]);
			}

			return result;
		}

		public static int[] FromStringToIntArray(this string sentence)
		{
			var splittedValues = sentence.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

			var values = splittedValues
				.Select(x => int.Parse(x.Trim()))
				.ToArray();

			return values;
		}

		public static string GetSubstringAfterSeparator(this string source, char separator)
		{
			for(var i = 0; i < source.Length; i++)
			{
				if(source[i] == separator)
				{
					return source.Substring(++i);
				}
			}

			return null;
		}
	}
}
