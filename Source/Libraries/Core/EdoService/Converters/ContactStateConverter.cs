using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Core.Domain.Clients;

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
