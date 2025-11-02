using System.Security.Cryptography;
using System.Text;

namespace VodovozInfrastructure.Cryptography
{
	public class MD5HexHashFromString : IMD5HexHashFromString
	{
		public string GetMD5HexHashFromString(string value)
		{
			var sb = new StringBuilder();
			var bytes = Encoding.UTF8.GetBytes(value);
			
			using(var md5Algorithm = MD5.Create())
			{
				var byteHash = md5Algorithm.ComputeHash(bytes);
				
				for(var i = 0; i < byteHash.Length; i++)
				{
					sb.Append(byteHash[i].ToString("x2"));
				}

				return sb.ToString();
			}
		}
	}
}
