using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Статус обновления клиента
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum CounterpartyUpdateStatus
	{
		Error = -1,
		CounterpartyNotFound = 0,
		CounterpartyUpdated = 1
	}
}
