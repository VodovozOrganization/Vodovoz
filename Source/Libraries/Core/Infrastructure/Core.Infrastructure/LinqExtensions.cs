using System.Collections.Generic;
using System.Linq;

namespace Core.Infrastructure
{
	public static class LinqExtensions
	{
		public static bool IsIn<T>(this T value, params T[] source )
		{
			return source.Contains(value);
		}

		public static bool IsIn<T>(this T value, IEnumerable<T> source)
		{
			return source.Contains(value);
		}

		public static bool IsNotIn<T>(this T value, params T[] source)
		{
			return !value.IsIn(source);
		}

		public static bool IsNotIn<T>(this T value, IEnumerable<T> source)
		{
			return !value.IsIn(source);
		}
	}
}
