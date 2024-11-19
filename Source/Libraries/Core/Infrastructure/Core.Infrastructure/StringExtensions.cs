using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Infrastructure
{
	public static class StringExtensions
	{
		public static bool IsNullOrWhiteSpace(this string s) 
		{  
			return string.IsNullOrWhiteSpace(s); 
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
