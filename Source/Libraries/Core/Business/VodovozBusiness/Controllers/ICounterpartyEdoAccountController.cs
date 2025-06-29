using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Client;

namespace VodovozBusiness.Controllers
{
	public interface ICounterpartyEdoAccountController
	{
		CounterpartyEdoAccount GetDefaultCounterpartyEdoAccountByOrganizationId(Counterparty client, int? organizationId);
		void AddDefaultEdoAccountsToNewCounterparty(Counterparty client);
	}
}
