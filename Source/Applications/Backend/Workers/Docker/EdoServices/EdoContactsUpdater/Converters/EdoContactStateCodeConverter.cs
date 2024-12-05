using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace EdoContactsUpdater.Converters
{
	public class EdoContactStateCodeConverter : IEdoContactStateCodeConverter
	{
		public ConsentForEdoStatus ConvertStateToConsentForEdoStatus(EdoContactStateCode stateCode)
		{
			switch(stateCode)
			{
				case EdoContactStateCode.Accepted:
					return ConsentForEdoStatus.Agree;
				case EdoContactStateCode.Sent:
					return ConsentForEdoStatus.Sent;
				case EdoContactStateCode.Rejected:
					return ConsentForEdoStatus.Rejected;
				default:
					return ConsentForEdoStatus.Unknown;
			}
		}
	}
}
