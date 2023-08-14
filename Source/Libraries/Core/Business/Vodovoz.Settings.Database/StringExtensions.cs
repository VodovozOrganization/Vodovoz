namespace Vodovoz.Settings.Database
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
	}
}
