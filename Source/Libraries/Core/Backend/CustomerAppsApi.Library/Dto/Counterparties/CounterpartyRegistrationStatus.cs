using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum CounterpartyRegistrationStatus : short
	{
		Error = -1,
		CounterpartyRegistered = 1,
		CounterpartyWithSameExternalIdExists = 2,
		CounterpartyWithSamePhoneNumberExists = 3
	}
}
