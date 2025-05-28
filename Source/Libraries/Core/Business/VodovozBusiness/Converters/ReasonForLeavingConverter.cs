using System;
using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace VodovozBusiness.Converters
{
	public class ReasonForLeavingConverter : IReasonForLeavingConverter
	{
		public ReasonForLeavingType ConvertReasonForLeavingToReasonForLeavingType(ReasonForLeaving reasonForLeaving)
		{
			switch(reasonForLeaving)
			{
				case ReasonForLeaving.Unknown:
					return ReasonForLeavingType.Unknown;
				case ReasonForLeaving.ForOwnNeeds:
				case ReasonForLeaving.Tender:
					return ReasonForLeavingType.ForOwnNeeds;
				case ReasonForLeaving.Resale:
					return ReasonForLeavingType.Resale;
				case ReasonForLeaving.Other:
					return ReasonForLeavingType.Other;
				default:
					throw new ArgumentOutOfRangeException(nameof(reasonForLeaving), reasonForLeaving, null);
			}
		}
	}
}
