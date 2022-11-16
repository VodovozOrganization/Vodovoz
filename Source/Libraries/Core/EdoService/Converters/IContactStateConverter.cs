using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Domain.Client;

namespace EdoService.Converters
{
	public interface IContactStateConverter
	{
		ConsentForEdoStatus ConvertStateToConsentForEdoStatus(ContactStateCode stateCode);
	}
}
