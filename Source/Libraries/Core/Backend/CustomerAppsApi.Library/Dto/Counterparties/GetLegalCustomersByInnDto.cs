using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	public class GetLegalCustomersByInnDto : GetLegalCustomersDto
	{
		public string Inn { get; set; }
	}
}
