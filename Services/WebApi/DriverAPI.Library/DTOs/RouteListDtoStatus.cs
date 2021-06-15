using System.Text.Json.Serialization;

namespace DriverAPI.Library.Models
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
