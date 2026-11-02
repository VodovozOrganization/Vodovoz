using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Sale;
using Vodovoz.Errors.Clients;
using Vodovoz.Specifications;
using Vodovoz.Tools.Orders;
using VodovozBusiness.Domain.Orders.Cart;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	/// <inheritdoc/>
	public class OnlineCartDistrictRulesGetter : IOnlineCartDistrictRulesGetter
	{
		private readonly CustomerCartWaterCounts _cartWaterCounts;

		public OnlineCartDistrictRulesGetter(
			CustomerCartWaterCounts cartWaterCounts
			)
		{
			_cartWaterCounts = cartWaterCounts ?? throw new ArgumentNullException(nameof(cartWaterCounts));
		}

		/// <inheritdoc/>
		public IWaterCount CartWaterCounts => _cartWaterCounts;

		/// <inheritdoc/>
		public Result<IEnumerable<DistrictRuleItemBase>> GetDeliveryRules(IDeliveryRulesRequestContext context)
		{
			var district = context.District;

			if(district is null)
			{
				return Result.Failure<IEnumerable<DistrictRuleItemBase>>(DeliveryPointErrors.CouldNotCalculateDeliveryBecauseDistrictNotFound());
			}
			
			if(CheckFreeDelivery(context))
			{
				return Result.Success<IEnumerable<DistrictRuleItemBase>>(Array.Empty<DistrictRuleItemBase>());
			}
			
			_cartWaterCounts.Initialize(context.CartItems);
			var deliveryRules = context.District.GetDeliveryRules(context.WeekDay, _cartWaterCounts, 0m);
			
			return Result.Success(deliveryRules);
		}

		private bool CheckFreeDelivery(IDeliveryRulesRequestContext context)
		{
			var isDeliveryForFree = CartFreeDeliverySpecification
				.Create()
				.IsSatisfiedBy(context);

			return isDeliveryForFree;
		}
	}
}
