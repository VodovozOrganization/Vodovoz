using System;

namespace CustomerAppsApi.Controllers
{
	public class CounterpartyContactInfoDto
	{
		public string PhoneNumber { get; set; }
		public Guid ExternalCounterpartyId { get; set; }
		public int CameFromId { get; set; }
	}
}
