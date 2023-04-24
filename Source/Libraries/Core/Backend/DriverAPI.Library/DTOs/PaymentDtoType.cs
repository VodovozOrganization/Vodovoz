using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
	// новый
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
