using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum CounterpartyIdentificationStatus : short
	{
		Error = -1,
		CounterpartyNotFound = 0,
		Success = 1,
		CounterpartyRegistered = 2,
		CounterpartyRegisteredWithoutEmail = 3,
		NeedManualHandling = 4
	}
}
