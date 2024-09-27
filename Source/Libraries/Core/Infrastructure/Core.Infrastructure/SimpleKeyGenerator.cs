using System;

namespace Core.Infrastructure
{
	public static class SimpleKeyGenerator
	{
		public static string GenerateKey(int length)
		{
			return DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
		}
	}
}
