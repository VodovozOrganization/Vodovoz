using System;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Core.Domain.Controllers
{
	public class CounterpartyEdoAccountEntityController : ICounterpartyEdoAccountEntityController
	{
		private readonly IOrganizationSettings _organizationSettings;

		public CounterpartyEdoAccountEntityController(
			IOrganizationSettings organizationSettings
			)
		{
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
		}
		
		public CounterpartyEdoAccountEntity GetDefaultCounterpartyEdoAccountByOrganizationId(
			CounterpartyEntity client, int? organizationId)
		{
			if(!organizationId.HasValue)
			{
				return client.DefaultEdoAccount(_organizationSettings.VodovozOrganizationId);
			}
			
			var counterpartyEdoAccount = client.DefaultEdoAccount(organizationId.Value);

			if(counterpartyEdoAccount is null)
			{
				counterpartyEdoAccount = client.DefaultEdoAccount(_organizationSettings.VodovozOrganizationId);
			}
			
			return counterpartyEdoAccount;
		}
	}
}
