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
	}
}
