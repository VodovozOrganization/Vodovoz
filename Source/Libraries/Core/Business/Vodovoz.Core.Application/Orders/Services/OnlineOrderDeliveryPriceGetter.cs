using System;
using Vodovoz.Core.Application.Orders.Delivery;
using Vodovoz.Core.Domain.Interfaces.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Specifications.Sale;

namespace Vodovoz.Core.Application.Orders.Services
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
			
			var isDeliveryForFree = IsFreeDelivery(onlineOrder);

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

		private bool IsFreeDelivery(OnlineOrder onlineOrder)
		{
			var isDeliveryForFree = false;

			switch(onlineOrder)
			{
				case OnlineOrderV1 onlineOrderV1:
					isDeliveryForFree = FreeDeliverySpecification
						.CreateForOnlineOrder(_paidDeliveryId)
						.IsSatisfiedBy(onlineOrderV1);
					break;
				case OnlineOrderV2 onlineOrderV2:
					isDeliveryForFree = OnlineOrderV2FreeDeliverySpecification
						.CreateForOnlineOrder(_paidDeliveryId)
						.IsSatisfiedBy(onlineOrderV2);
					break;
			}

			return isDeliveryForFree;
		}
	}
}
