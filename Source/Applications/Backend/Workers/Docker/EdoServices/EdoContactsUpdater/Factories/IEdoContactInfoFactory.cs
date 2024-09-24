using System;
using TaxcomEdo.Contracts.Contacts;
using TaxcomEdo.Contracts.Counterparties;

namespace EdoContactsUpdater.Factories
{
	public interface IEdoContactInfoFactory
	{
		EdoContactInfo CreateEdoContactInfo(
			string inn,
			string edxClientId,
			EdoContactStateCode stateCode,
			DateTime stateChanged);
	}
}
