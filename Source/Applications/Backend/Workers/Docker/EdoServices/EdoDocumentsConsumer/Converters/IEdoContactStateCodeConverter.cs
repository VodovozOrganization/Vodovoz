using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Domain.Client;

namespace EdoDocumentsConsumer.Converters
{
	public interface IEdoContactStateCodeConverter
	{
		ConsentForEdoStatus ConvertStateToConsentForEdoStatus(EdoContactStateCode stateCode);
	}
}
