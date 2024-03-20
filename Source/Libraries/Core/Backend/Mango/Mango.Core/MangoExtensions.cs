using System;

namespace Mango.Core
{
	public static class MangoExtensions
	{
		public static uint? ParseExtension(this string extension)
		{
			return uint.TryParse(extension, out var i) ? (uint?)i : null;
		}

		public static DateTime ParseTimestamp(this long timestamp)
		{
			var offset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
			return offset.LocalDateTime;
		}
	}
}
