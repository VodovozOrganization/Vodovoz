using System;
using TaxcomEdo.Contracts.Contacts;
using TaxcomEdo.Contracts.Counterparties;

namespace EdoContactsUpdater.Factories
{
	public sealed class EdoContactInfoFactory : IEdoContactInfoFactory
	{
		public EdoContactInfo CreateEdoContactInfo(
			string inn,
			string edxClientId,
			EdoContactStateCode stateCode,
			DateTime stateChanged)
		{
			return new EdoContactInfo
			{
				Inn = inn,
				EdxClientId = edxClientId,
				StateCode = stateCode,
				StateChanged = stateChanged
			};
		}
	}
}
