using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Controllers
{
	public class CounterpartyContactInfoDto
	{
		public string PhoneNumber { get; set; }
		public int ExternalCounterpartyId { get; set; }
		public CounterpartyFrom CounterpartyFrom { get; set; }
	}
}
