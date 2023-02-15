using System.Text.Json.Serialization;

namespace CustomerAppsApi.Dto
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum CounterpartyUpdateStatus
	{
		Error = -1,
		CounterpartyNotFound = 0,
		CounterpartyUpdated = 1
	}
}
