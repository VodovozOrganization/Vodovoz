using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Domain.Client;

namespace EdoService.Library.Converters
{
	public class ContactStateConverter : IContactStateConverter
	{
		public ConsentForEdoStatus ConvertStateToConsentForEdoStatus(ContactStateCode stateCode)
		{
			switch(stateCode)
			{
				case ContactStateCode.Accepted:
					return ConsentForEdoStatus.Agree;
				case ContactStateCode.Sent:
					return ConsentForEdoStatus.Sent;
				case ContactStateCode.Rejected:
					return ConsentForEdoStatus.Rejected;
				default:
					return ConsentForEdoStatus.Unknown;
			}
		}
	}
}
