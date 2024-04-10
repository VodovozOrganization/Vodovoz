using Google.Protobuf.WellKnownTypes;
using System;

namespace Mango.Service.Extensions
{
	public static class MangoExtensions
	{
		public static Timestamp ParseProtoTimestamp(this long timestamp)
		{
			var offset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
			return Timestamp.FromDateTimeOffset(offset);
		}
	}
}
