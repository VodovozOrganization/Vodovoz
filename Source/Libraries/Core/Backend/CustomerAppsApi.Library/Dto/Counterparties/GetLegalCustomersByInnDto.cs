using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	public abstract class GetLegalCustomersByInnDto : GetLegalCustomersDto
	{
		public string Inn { get; set; }
	}
}
