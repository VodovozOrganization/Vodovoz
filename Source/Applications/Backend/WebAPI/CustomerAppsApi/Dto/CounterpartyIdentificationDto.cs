using CustomerAppsApi.Controllers;

namespace CustomerAppsApi.Models
{
	public class CounterpartyIdentificationDto
	{
		public RegisteredNaturalCounterpartyDto RegisteredNaturalCounterpartyDto { get; set; }
		public string ErrorDescription { get; set; }
		public CounterpartyIdentificationStatus CounterpartyIdentificationStatus { get; set; }
	}
}