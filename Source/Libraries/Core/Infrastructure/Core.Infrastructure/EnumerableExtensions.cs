using System.Collections.Generic;
using System.Text;

namespace Core.Infrastructure
{
	public static class EnumerableExtensions
	{
		public static string ToStringValue<T>(this IList<T> source, char separator)
		{
			var sb = new StringBuilder();

			for(var i = 0; i < source.Count; i++)
			{
				sb.Append(source[i]);

				if(i < source.Count - 1)
				{
					sb.Append(separator);
				}
			}
			
			return sb.ToString();
		}
		
		public static string ToStringValue<T>(this IEnumerable<T> source, char separator)
		{
			var sb = new StringBuilder();

			foreach(var item in source)
			{
				sb.Append(item);
				sb.Append(separator);
			}
			
			return sb
				.ToString()
				.TrimEnd(separator);
		}
	}
}
