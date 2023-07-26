using System;
using System.Text.Json.Serialization;

namespace DriverAPI.Library.Deprecated2.DTOs
{
	[Obsolete("Будет удален с прекращением поддержки API v2")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PaymentDtoType
	{
		Cash,
		Terminal,
		DriverApplicationQR,
		Paid,
		Cashless,
	}
}
