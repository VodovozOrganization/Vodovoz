using System;
using CustomerAppsApi.Library.Dto.Edo;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Extensions
{
	public static class WaterPurposeOfPurchaseExtensions
	{
		public static ReasonForLeaving ToReasonForLeaving(this WaterPurposeOfPurchase source)
		{
			return source switch
			{
				WaterPurposeOfPurchase.OwnNeeds => ReasonForLeaving.ForOwnNeeds,
				WaterPurposeOfPurchase.Resale => ReasonForLeaving.Resale,
				_ => throw new ArgumentOutOfRangeException(nameof(source), @"Неизвестная причина покупки воды")
			};
		}
	}
}
