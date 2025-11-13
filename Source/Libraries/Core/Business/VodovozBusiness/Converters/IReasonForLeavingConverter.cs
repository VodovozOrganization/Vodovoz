using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace VodovozBusiness.Converters
{
	public interface IReasonForLeavingConverter
	{
		ReasonForLeavingType ConvertReasonForLeavingToReasonForLeavingType(ReasonForLeaving reasonForLeaving);
	}
}
