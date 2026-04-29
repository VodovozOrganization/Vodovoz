using System;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Specifications;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Core.Application.Orders.Services
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
			var isDeliveryForFree = FreeDeliverySpecification
				.CreateForOnlineOrder(_paidDeliveryId)
				.IsSatisfiedBy(onlineOrder);
			
			if(isDeliveryForFree)
			{
				return 0;
			}
			
			var district = onlineOrder.DeliveryPoint?.District;

			if(district is null)
			{
				return 0;
			}
			
			_onlineOrderStateKey.InitializeFields(onlineOrder);
			var price = district.GetDeliveryPrice(_onlineOrderStateKey, 0m);
			return price;
		}
	}
}
