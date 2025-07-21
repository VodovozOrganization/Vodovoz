using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Infrastructure
{
	public static class StringExtensions
	{
		public static bool IsNullOrWhiteSpace(this string s) 
		{
			return string.IsNullOrWhiteSpace(s); 
		}
		
		public static IEnumerable<int> ParseNumbers(this string s)
		{
			const string pattern = @"\d+";
			return (from Match match in Regex.Matches(s, pattern) select int.Parse(match.Value)).ToList();
		}
		
		public static T? TryParseAsEnum<T>(this string value) where T : struct
		{
			if(string.IsNullOrEmpty(value))
			{
				return new T?();
			}

			return !Enum.TryParse<T>(value, true, out var result) ? new T?() : result;
		}
	}
}
