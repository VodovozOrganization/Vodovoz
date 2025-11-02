using System.Security.Cryptography;
using System.Text;

namespace VodovozInfrastructure.Cryptography
{
	public static class HashCompute
	{
		public static string GetSha512HashString(string input, bool isToLower = true)
		{
			var bytes = Encoding.UTF8.GetBytes(input);

			using(var sha512 = SHA512.Create())
			{
				var hashedBytes = sha512.ComputeHash(bytes);

				var hashedInputStringBuilder = new StringBuilder(128);

				foreach(var b in hashedBytes)
				{
					hashedInputStringBuilder.Append(b.ToString("X2"));
				}

				var result = hashedInputStringBuilder.ToString();

				if(isToLower)
				{
					result = result.ToLower();
				}

				return result;
			}
		}
	}
}
