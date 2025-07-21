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
				return client.DefaultEdoAccount(_organizationSettings.VodovozOrganizationId);
			}
			
			var counterpartyEdoAccount = client.DefaultEdoAccount(organizationId.Value);

			if(counterpartyEdoAccount is null)
			{
				counterpartyEdoAccount = client.DefaultEdoAccount(_organizationSettings.VodovozOrganizationId);
			}
			
			return counterpartyEdoAccount;
		}
		
		public void AddDefaultEdoAccountsToNewCounterparty(Counterparty client)
		{
			if(client.Id != 0)
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
	}
}
