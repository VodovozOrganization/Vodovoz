using System.IO;
using System.Security.Cryptography;

namespace Vodovoz.Core.Domain.Extensions
{
	public static class StreamExtensions
	{
		public static string GetMd5Hash(this Stream stream)
			=> MD5
				.Create()
				.ComputeHash(stream)
				.ConvertToUtf8String();

		public static string GetSha256Hash(this Stream stream)
			=> SHA256
				.Create()
				.ComputeHash(stream)
				.ConvertToUtf8String();
	}
}
