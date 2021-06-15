using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PaymentDtoType
	{
		Cash,
		Cashless,
		Terminal,
		ByCard,
		ByCardFromSms,
		Payed
	}
}
