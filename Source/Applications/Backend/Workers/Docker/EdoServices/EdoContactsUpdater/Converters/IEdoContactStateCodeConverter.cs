using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Domain.Client;

namespace EdoContactsUpdater.Converters
{
	public interface IEdoContactStateCodeConverter
	{
		ConsentForEdoStatus ConvertStateToConsentForEdoStatus(EdoContactStateCode stateCode);
	}
}
