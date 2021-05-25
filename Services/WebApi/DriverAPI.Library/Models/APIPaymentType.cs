using System.Text.Json.Serialization;

namespace DriverAPI.Library.Models
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum APIPaymentType
	{
		Cash,
		Cashless,
		Terminal,
		ByCard,
		ByCardFromSms,
		Payed
	}
}
