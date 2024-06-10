using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	public class CounterpartyIdentificationDto
	{
		public RegisteredNaturalCounterpartyDto RegisteredNaturalCounterpartyDto { get; set; }
		public string ErrorDescription { get; set; }

		[JsonPropertyName("counterpartyIdentificationStatus")]
		public CounterpartyIdentificationStatus CounterpartyIdentificationStatus { get; set; }
	}
}
