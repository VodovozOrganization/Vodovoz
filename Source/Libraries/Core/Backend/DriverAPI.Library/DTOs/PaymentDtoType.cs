using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
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
