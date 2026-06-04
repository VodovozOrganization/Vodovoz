using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Errors.Clients;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class OrderDeliveryPriceGetter : IOrderDeliveryPriceGetter
	{
		private readonly OrderStateKey _orderStateKey;
		private readonly int _paidDeliveryId;

		public OrderDeliveryPriceGetter(
			INomenclatureSettings nomenclatureSettings,
			OrderStateKey orderStateKey)
		{
			_orderStateKey = orderStateKey ?? throw new ArgumentNullException(nameof(orderStateKey));
			_paidDeliveryId =
				(nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings)))
				.PaidDeliveryNomenclatureId;
		}
		
		public Result<decimal> GetDeliveryPrice(IUnitOfWork unitOfWork, Order order)
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
				return Result.Success(0m);
			}

			#endregion

			District district = null;
			
			if(order.DeliveryPoint != null)
			{
				if(order.DeliveryPoint.District is null)
				{
					return Result.Failure<decimal>(DeliveryPointErrors.CouldNotCalculateDeliveryBecauseDistrictNotFound(order.DeliveryPoint.Id));
				}

				district = unitOfWork.GetById<District>(order.DeliveryPoint.District.Id);
			}

			_orderStateKey.InitializeFields(order);

			var price =
				district?.GetDeliveryPrice(_orderStateKey, order.ObservableOrderItems
					.Sum(x => x.Nomenclature?.OnlineStoreExternalId != null ? x.ActualSum : 0m))
				?? 0m;

			return Result.Success(price);
		}
	}
}
