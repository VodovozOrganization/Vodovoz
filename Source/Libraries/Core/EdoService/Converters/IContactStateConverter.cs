using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Core.Domain.Clients;

namespace EdoService.Library.Converters
{
	public interface IContactStateConverter
	{
		ConsentForEdoStatus ConvertStateToConsentForEdoStatus(ContactStateCode stateCode);
	}
}
