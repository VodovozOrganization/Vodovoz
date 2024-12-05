using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace EdoContactsUpdater.Converters
{
	public interface IEdoContactStateCodeConverter
	{
		ConsentForEdoStatus ConvertStateToConsentForEdoStatus(EdoContactStateCode stateCode);
	}
}
