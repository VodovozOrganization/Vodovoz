using System;
using System.Linq;
using Vodovoz.Core.Domain.Interfaces.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Sale;
using Vodovoz.Errors.Clients;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Specifications;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	public class OrderDeliveryPriceGetter : IDeliveryPriceGetter<OrderDeliveryPriceContext>
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
		
		public Result<decimal> GetDeliveryPrice(IDeliveryPriceGetterContext<OrderDeliveryPriceContext> context)
		{
			var order = context.Data.Order;
			var isDeliveryForFree = OrderFreeDeliverySpecification
				.Create(_paidDeliveryId)
				.IsSatisfiedBy(order);

			if(isDeliveryForFree)
			{
				return Result.Success(0m);
			}

			District district = null;
			
			if(order.DeliveryPoint != null)
			{
				if(order.DeliveryPoint.District is null)
				{
					return Result.Failure<decimal>(DeliveryPointErrors.CouldNotCalculateDeliveryBecauseDistrictNotFound(order.DeliveryPoint.Id));
				}

				district = context.Data.UnitOfWork.GetById<District>(order.DeliveryPoint.District.Id);
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
