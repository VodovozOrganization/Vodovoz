using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Sale;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Specifications;
using Vodovoz.Tools.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	/// <inheritdoc/>
	public class OnlineOrderFromCartDistrictRulesGetter : IOnlineOrderFromCartDistrictRulesGetter
	{
		private readonly int _paidDeliveryId;

		public OnlineOrderFromCartDistrictRulesGetter(
			INomenclatureSettings nomenclatureSettings,
			OnlineOrderFromCartStateKey onlineOrderFromCartStateKey)
		{
			OnlineOrderFromCartStateKey = onlineOrderFromCartStateKey ?? throw new ArgumentNullException(nameof(onlineOrderFromCartStateKey));
			_paidDeliveryId =
				(nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings)))
				.PaidDeliveryNomenclatureId;
		}
		
		public OnlineOrderFromCartStateKey OnlineOrderFromCartStateKey { get; }
		
		/// <inheritdoc/>
		public Result<decimal> GetDeliveryPrice(IOnlineOrderFromCart onlineOrder)
		{
			if(CheckFreeDelivery(onlineOrder))
			{
				return Result.Success(0m);
			}
			
			var district = onlineOrder.DeliveryPoint?.District;

			if(district is null)
			{
				return Result.Failure<decimal>(new Error("500", "Не найден логистический район"));
			}
			
			OnlineOrderFromCartStateKey.InitializeFields(onlineOrder);
			var price = district.GetDeliveryPrice(OnlineOrderFromCartStateKey, 0m);
			return Result.Success(price);
		}

		/// <inheritdoc/>
		public Result<IEnumerable<DistrictRuleItemBase>> GetDeliveryRules(IOnlineOrderFromCart onlineOrder)
		{
			if(CheckFreeDelivery(onlineOrder))
			{
				return Result.Success<IEnumerable<DistrictRuleItemBase>>(null);
			}

			var district = onlineOrder.DeliveryPoint?.District;

			if(district is null)
			{
				return Result.Failure<IEnumerable<DistrictRuleItemBase>>(new Error("500", "Не найден логистический район"));
			}
			
			OnlineOrderFromCartStateKey.InitializeFields(onlineOrder);
			var deliveryRules = district.GetDeliveryRules(OnlineOrderFromCartStateKey, 0m);
			return Result.Success(deliveryRules);
		}

		private bool CheckFreeDelivery(IOnlineOrderFromCart onlineOrder)
		{
			var isDeliveryForFree = FreeDeliverySpecification
				.CreateForOnlineOrder(_paidDeliveryId)
				.IsSatisfiedBy(onlineOrder);

			return isDeliveryForFree;
		}
	}
}
