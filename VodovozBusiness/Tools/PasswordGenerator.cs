using System;
using System.Linq;

namespace Vodovoz.Tools
{
	public class PasswordGenerator : IPasswordGenerator
	{
		public string GeneratePassword(int length)
		{
			var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			var result = new string(
				Enumerable.Repeat(chars, length)
						  .Select(s => s[random.Next(s.Length)])
						  .ToArray());
			return result;
		}
	}
}
