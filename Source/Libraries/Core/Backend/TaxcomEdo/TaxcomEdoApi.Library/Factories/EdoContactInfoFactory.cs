using TaxcomEdo.Contracts.Counterparties;

namespace TaxcomEdoApi.Library.Factories
{
	public sealed class EdoContactInfoFactory : IEdoContactInfoFactory
	{
		public EdoContactInfo CreateEdoContactInfo(string inn, string edxClientId, string stateCode)
		{
			return new EdoContactInfo
			{
				Inn = inn,
				EdxClientId = edxClientId,
				StateCode = stateCode
			};
		}
	}
}
