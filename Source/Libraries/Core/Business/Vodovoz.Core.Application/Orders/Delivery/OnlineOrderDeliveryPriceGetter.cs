using System;
using Vodovoz.Core.Domain.Interfaces.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Specifications;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	public class OnlineOrderDeliveryPriceGetter : IDeliveryPriceGetter<OnlineOrderDeliveryPriceContext>
	{
		private readonly OnlineOrderStateKey _onlineOrderStateKey;
		private readonly int _paidDeliveryId;

		public OnlineOrderDeliveryPriceGetter(
			INomenclatureSettings nomenclatureSettings,
			OnlineOrderStateKey onlineOrderStateKey)
		{
			_onlineOrderStateKey = onlineOrderStateKey ?? throw new ArgumentNullException(nameof(onlineOrderStateKey));
			_paidDeliveryId =
				(nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings)))
				.PaidDeliveryNomenclatureId;
		}
		
		public Result<decimal> GetDeliveryPrice(IDeliveryPriceGetterContext<OnlineOrderDeliveryPriceContext> context)
		{
			var onlineOrder = context.Data.OnlineOrder;
			
			var isDeliveryForFree = FreeDeliverySpecification
				.CreateForOnlineOrder(_paidDeliveryId)
				.IsSatisfiedBy(onlineOrder);
			
			if(isDeliveryForFree)
			{
				return Result.Success(0m);
			}
			
			var district = onlineOrder.DeliveryPoint?.District;

			if(district is null)
			{
				return Result.Success(0m);
			}
			
			_onlineOrderStateKey.InitializeFields(onlineOrder);
			var price = district.GetDeliveryPrice(_onlineOrderStateKey, 0m);
			return Result.Success(price);
		}
	}
}
