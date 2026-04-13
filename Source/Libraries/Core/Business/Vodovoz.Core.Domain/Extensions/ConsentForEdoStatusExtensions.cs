using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain.Extensions
{
	public static class ConsentForEdoStatusExtensions
	{
		public static ConsentForEdoStatus ToConsentEdoStatus(this EdoContactStateCode stateCode)
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
