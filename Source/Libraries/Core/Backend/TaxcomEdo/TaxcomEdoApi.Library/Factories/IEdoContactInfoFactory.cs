using TaxcomEdo.Contracts.Counterparties;

namespace TaxcomEdoApi.Library.Factories
{
	public interface IEdoContactInfoFactory
	{
		EdoContactInfo CreateEdoContactInfo(string inn, string edxClientId, string stateCode);
	}
}
