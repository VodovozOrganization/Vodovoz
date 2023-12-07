using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.Dto
{
	public class CounterpartyIdentificationDto
	{
		public RegisteredNaturalCounterpartyDto RegisteredNaturalCounterpartyDto { get; set; }
		public string ErrorDescription { get; set; }

		[JsonPropertyName("counterpartyIdentificationStatus")]
		public CounterpartyIdentificationStatus CounterpartyIdentificationStatus { get; set; }
	}
}
