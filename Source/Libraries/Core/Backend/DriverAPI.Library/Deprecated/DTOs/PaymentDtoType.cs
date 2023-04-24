using System;
using System.Text.Json.Serialization;

namespace DriverAPI.Library.Deprecated.DTOs
{
	[Obsolete("Будет удален с прекращением поддержки API v1")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PaymentDtoType
	{
		Cash,
		Cashless,
		Barter,
		Terminal,
		ByCard,
		ByCardFromSms,
		ContractDocumentation
	}
}
