using System;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	public class CounterpartyContactInfoDto : ExternalCounterpartyDto
	{
		public string PhoneNumber { get; set; }
		public int CameFromId { get; set; }
	}
}
