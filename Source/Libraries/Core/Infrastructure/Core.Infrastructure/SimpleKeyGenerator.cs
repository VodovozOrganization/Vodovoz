using System;
using System.Linq;
using System.Security.Cryptography;

namespace Core.Infrastructure
{
	public static class SimpleKeyGenerator
	{
		public static string GenerateKey(int length)
		{
			var bytes = new byte[length];

			RandomNumberGenerator.Create().GetBytes(bytes);

			string base64String = Convert.ToBase64String(bytes)
				.Replace("+", "")
				.Replace("/", "");

			return base64String.Substring(0, length).Trim('=');
		}
	}
}
