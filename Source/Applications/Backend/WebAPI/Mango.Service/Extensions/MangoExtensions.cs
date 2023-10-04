using Google.Protobuf.WellKnownTypes;
using System;

namespace Mango.Service.Extensions
{
	public static class MangoExtensions
	{
		public static uint? ParseExtension(this string extension)
		{
			return uint.TryParse(extension, out var i) ? (uint?)i : null;
		}

		public static Timestamp ParseTimestamp(this long timestamp)
		{
			var offset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
			return Timestamp.FromDateTimeOffset(offset);
		}

		public static CallState ParseCallState(this string callState)
		{
			return System.Enum.Parse<CallState>(callState);
		}

	}
}
