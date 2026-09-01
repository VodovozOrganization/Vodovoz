using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;

namespace VodovozBusiness.Extensions
{
	public static class DiscountReasonExtensions
	{
		public static IEnumerable<DiscountReasonBase> ToDiscountReasonBases(
			this IEnumerable<int> discountReasonIds,
			IUnitOfWork uow,
			IDiscountReasonRepository discountReasonRepository)
		{
			if(discountReasonIds is null || !discountReasonIds.Any())
			{
				return Array.Empty<DiscountReasonBase>();
			}

			return discountReasonRepository.GetDiscountReasons(uow, discountReasonIds);
		}
	}
}
