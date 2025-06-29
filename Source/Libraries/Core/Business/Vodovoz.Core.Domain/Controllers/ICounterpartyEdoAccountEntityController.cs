using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain.Controllers
{
	public interface ICounterpartyEdoAccountEntityController
	{
		CounterpartyEdoAccountEntity GetDefaultCounterpartyEdoAccountByOrganizationId(CounterpartyEntity client, int? organizationId);
	}
}
