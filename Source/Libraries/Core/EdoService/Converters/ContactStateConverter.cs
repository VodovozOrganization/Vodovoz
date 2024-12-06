using TaxcomEdo.Contracts.Counterparties;
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

		public EdoContactStateCode ConvertStateToEdoContactStateCode(ContactStateCode stateCode)
		{
			switch(stateCode)
			{
				case ContactStateCode.Incoming:
					return EdoContactStateCode.Incoming;
				case ContactStateCode.Sent:
					return EdoContactStateCode.Sent;
				case ContactStateCode.Accepted:
					return EdoContactStateCode.Accepted;
				case ContactStateCode.Rejected:
					return EdoContactStateCode.Rejected;
				case ContactStateCode.Error:
				default:
					return EdoContactStateCode.Error;
			}
		}
	}
}
