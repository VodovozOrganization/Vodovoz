using System;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	public class CounterpartyContactInfoDto
	{
		public string PhoneNumber { get; set; }
		public Guid ExternalCounterpartyId { get; set; }
		public int CameFromId { get; set; }
	}
}
