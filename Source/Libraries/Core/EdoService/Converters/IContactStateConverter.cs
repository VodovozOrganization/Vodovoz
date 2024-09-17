using TaxcomEdo.Contracts.Counterparties;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Domain.Client;

namespace EdoService.Library.Converters
{
	public interface IContactStateConverter
	{
		ConsentForEdoStatus ConvertStateToConsentForEdoStatus(ContactStateCode stateCode);
		EdoContactStateCode ConvertStateToEdoContactStateCode(ContactStateCode stateCode);
	}
}
