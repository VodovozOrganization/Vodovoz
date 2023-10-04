using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Mango.Core.Sign
{
	public class SignGenerator : ISignGenerator
	{
		public string GetSign(string vpbxApiKey, string vpbxApiSalt, string json)
		{
			if(string.IsNullOrEmpty(vpbxApiKey))
			{
				throw new ArgumentException($"{nameof(vpbxApiKey)} was empty");
			}

			if(string.IsNullOrEmpty(vpbxApiSalt))
			{
				throw new ArgumentException($"{nameof(vpbxApiSalt)} was empty");
			}

			string common;
			if(string.IsNullOrWhiteSpace(json))
			{
				common = vpbxApiKey + "{}" + vpbxApiSalt;
			}
			else
			{
				common = vpbxApiKey + json + vpbxApiSalt;
			}

			string sign = null;
			using(SHA256 algorithm = SHA256.Create())
			{
				var content = common.Replace("\r\n", "\n");
				var bytes = Encoding.UTF8.GetBytes(content);
				byte[] data = algorithm.ComputeHash(bytes);
				var sBuilder = new StringBuilder();
				for(int i = 0; i < data.Length; i++)
				{
					sBuilder.Append(data[i].ToString("x2"));
				}

				sign = sBuilder.ToString();
				Console.WriteLine($"sign: {sign}");
			}

			return sign;
		}
	}
}
