using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.Application.Clients
{
	public class CounterpartyEdoAccountController : ICounterpartyEdoAccountController
	{
		private readonly IOrganizationSettings _organizationSettings;

		public CounterpartyEdoAccountController(
			IOrganizationSettings organizationSettings
			)
		{
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
		}
		
		public CounterpartyEdoAccount GetDefaultCounterpartyEdoAccountByOrganizationId(Counterparty client, int? organizationId)
		{
			if(!organizationId.HasValue)
			{
				return GetDefaultEmptyEdoAccount(client);
			}
			
			var counterpartyEdoAccount = client.DefaultEdoAccount(organizationId.Value);
			return counterpartyEdoAccount ?? GetDefaultEmptyEdoAccount(client);
		}
		
		public void AddDefaultEdoAccountsToCounterparty(Counterparty client)
		{
			if(client.CounterpartyEdoAccounts.Any())
			{
				return;
			}
			
			var defaultEdoAccountsOrganizationsIds = new[]
			{
				_organizationSettings.VodovozOrganizationId,
				_organizationSettings.KulerServiceOrganizationId
			};
			
			foreach(var organizationId in defaultEdoAccountsOrganizationsIds)
			{
				if(client.CounterpartyEdoAccounts.Any(x => x.OrganizationId == organizationId))
				{
					continue;
				}

				var edoAccount = CounterpartyEdoAccount.Create(
					client,
					null,
					null,
					organizationId,
					true
					);
				
				client.CounterpartyEdoAccounts.Add(edoAccount);
			}
		}
		
		private CounterpartyEdoAccount GetDefaultEmptyEdoAccount(Counterparty client) =>
			CounterpartyEdoAccount.Create(client, null, null, 0, true);
	}
}
