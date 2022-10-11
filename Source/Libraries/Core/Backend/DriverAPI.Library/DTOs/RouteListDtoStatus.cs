using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum RouteListDtoStatus
	{
		New,
		Confirmed,
		InLoading,
		EnRoute,
		Delivered,
		OnClosing,
		MileageCheck,
		Closed
	}
}
