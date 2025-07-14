namespace Core.Infrastructure
{
	public static class CharExtensions
	{
		public static bool IsDotOrComma(this char c)
		{
			return c == '.' || c == ',';
		}
	}
}
