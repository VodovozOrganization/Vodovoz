using System.Security.Cryptography;
using System.Text;

namespace Vodovoz.Core.Domain.Extensions
{
	public static class ByteArrayExtensions
	{
		public static string GetMd5Hash(this byte[] data)
			=> MD5
				.Create()
				.ComputeHash(data)
				.ConvertToUtf8String();

		public static string GetSha256Hash(this byte[] data)
			=> SHA256
				.Create()
				.ComputeHash(data)
				.ConvertToUtf8String();

		public static string ConvertToUtf8String(this byte[] data)
			=> Encoding.UTF8.GetString(data);
	}
}
