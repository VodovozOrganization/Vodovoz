using System.Security.Cryptography;
using System.Text;


namespace Sms.External.SmsRu
{
	public partial class SmsRuSendController
	{
		internal class HashCodeHelper
		{
			public static string ComputeHash(string input, HashAlgorithm algorithm)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(input);
				return ConvertersHelper.ByteArrayToHex(algorithm.ComputeHash(bytes));
			}

			public static string GetSHA512Hash(string inputString)
			{
				using(SHA512 sHA = new SHA512Managed())
				{
					byte[] bytes = Encoding.UTF8.GetBytes(inputString);
					return ConvertersHelper.ByteArrayToHex(sHA.ComputeHash(bytes));
				}
			}
		}
	}
}
