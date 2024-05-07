using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class DeliveryPriceCalculator : IDeliveryPriceCalculator
	{
		private readonly int _paidDeliveryId;

		public DeliveryPriceCalculator(INomenclatureSettings nomenclatureSettings)
		{
			_paidDeliveryId =
				(nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings)))
				.PaidDeliveryNomenclatureId;
		}
		
		public decimal CalculateDeliveryPrice(IUnitOfWork unitOfWork, Order order)
		{
			#region перенести всё это в OrderStateKey

			var isDeliveryForFree =
				order.SelfDelivery
					|| order.OrderAddressType == OrderAddressType.Service
					|| order.DeliveryPoint.AlwaysFreeDelivery
					|| order.ObservableOrderItems
						.Any(n => n.Nomenclature.Category == NomenclatureCategory.spare_parts)
					|| !order.ObservableOrderItems.Any(n => n.Nomenclature.Id != _paidDeliveryId)
					&& (order.BottlesReturn > 0
						|| order.ObservableOrderEquipments.Any()
						|| order.ObservableOrderDepositItems.Any());

			if(isDeliveryForFree)
			{
				return default;
			}

			#endregion

			var district = order.DeliveryPoint != null
				? unitOfWork.GetById<District>(order.DeliveryPoint.District.Id)
				: null;

			var orderKey = new OrderStateKey(order);

			var price =
				district?.GetDeliveryPrice(orderKey, order.ObservableOrderItems
					.Sum(x => x.Nomenclature?.OnlineStoreExternalId != null ? x.ActualSum : 0m))
				?? 0m;

			return price;
		}
		
		public decimal CalculateDeliveryPrice(OnlineOrder onlineOrder)
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
			
			var onlineOrderKey = new OnlineOrderStateKey(onlineOrder);
			var price = district.GetDeliveryPrice(onlineOrderKey, 0m);
			return price;
		}
	}
}
