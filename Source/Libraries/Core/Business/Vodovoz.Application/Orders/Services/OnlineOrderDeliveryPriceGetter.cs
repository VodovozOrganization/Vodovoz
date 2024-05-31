using System;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OnlineOrderDeliveryPriceGetter : IOnlineOrderDeliveryPriceGetter
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
		
		public decimal GetDeliveryPrice(OnlineOrder onlineOrder)
		{
			var isDeliveryForFree =
				onlineOrder.IsSelfDelivery
				|| (onlineOrder.DeliveryPoint != null && onlineOrder.DeliveryPoint.AlwaysFreeDelivery)
				|| !onlineOrder.OnlineOrderItems.Any(n => n.Nomenclature != null && n.Nomenclature.Id != _paidDeliveryId);
			
			if(isDeliveryForFree)
			{
				return default;
			}
			
			var district = onlineOrder.DeliveryPoint?.District;

			if(district is null)
			{
				return default;
			}
			
			_onlineOrderStateKey.InitializeFields(onlineOrder);
			var price = district.GetDeliveryPrice(_onlineOrderStateKey, 0m);
			return price;
		}
	}
}
