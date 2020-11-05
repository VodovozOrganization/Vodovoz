using System;
using System.Security.Cryptography;
using System.Text;

namespace MangoService
{
    public static class MangoSignHelper
    {
        public static string GetSign(string vpbx_api_key, string json, string vpbx_api_salt)
        {
            if (String.IsNullOrEmpty(vpbx_api_key))
                throw new ArgumentException($"{nameof(vpbx_api_key)} was empty");
            if (String.IsNullOrEmpty(vpbx_api_salt))
                throw new ArgumentException($"{nameof(vpbx_api_salt)} was empty");
			string common = null;

			if(String.IsNullOrWhiteSpace(json))
				common = vpbx_api_key + "{}" + vpbx_api_salt;
			else
				common = vpbx_api_key + json + vpbx_api_salt;
			string sign = null;
            using (SHA256 algorithm = SHA256.Create())
            {
                byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(common));
                var sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                sign = sBuilder.ToString();
            }

            return sign;
        }
    }
}