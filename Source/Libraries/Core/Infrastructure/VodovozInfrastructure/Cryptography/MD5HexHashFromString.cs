using System.Security.Cryptography;
using System.Text;

namespace VodovozInfrastructure.Cryptography
{
	public class MD5HexHashFromString : IMD5HexHashFromString
	{
		private readonly StringBuilder _sb;

		public MD5HexHashFromString()
		{
			_sb = new StringBuilder();
		}
		
		public string GetMD5HexHashFromString(string value)
		{
			_sb.Clear();
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			using(var md5Algoritm = MD5.Create())
			{
				byte[] byteHash = md5Algoritm.ComputeHash(bytes);
				
				for(var i = 0; i < byteHash.Length; i++)
				{
					_sb.Append(byteHash[i].ToString("x2"));
				}

				return _sb.ToString();
			}
		}
	}
}
