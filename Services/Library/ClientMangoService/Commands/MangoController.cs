using System;
using System.Security.Cryptography;
using System.Text;

namespace ClientMangoService.Commands
{
	public class MangoController
	{
		public MangoController()
		{

		}
		private static string GetSign(string vpbx_api_key, string json, string vpbx_api_salt)
		{
			if(vpbx_api_key == "")
				throw new ArgumentException("vpbx_api_key was \"\"");
			if(json == "")
				throw new ArgumentException("json was \"\"");
			if(vpbx_api_salt == "")
				throw new ArgumentException("vpbx_api_salt was \"\"");

			string common = vpbx_api_key + json + vpbx_api_salt;
			string sign = "";
			using(SHA256 algorithm = SHA256.Create()) {
				try {
					byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(common));
					var sBuilder = new StringBuilder();

					for(int i = 0; i < data.Length; i++) {
						sBuilder.Append(data[i].ToString("x2"));
					}
					sign = sBuilder.ToString();
				} catch(ArgumentNullException e) {
					Console.WriteLine(e.Message);
					return "";
				}
				return sign;
			}
			return "";
		}
	}
}
