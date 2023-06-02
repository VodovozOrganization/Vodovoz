namespace Vodovoz.Tools
{
	public static class StringUtils
	{
		public static string CapitalizeSentence(this string sentence)
			=> char.ToUpper(sentence[0]) + sentence.Substring(1);
	}
}
