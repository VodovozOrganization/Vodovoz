using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PaymentDtoType
	{
		Cash,
		TerminalQR,
		DriverApplicationQR,
		Paid,
		Cashless,
	}
}
