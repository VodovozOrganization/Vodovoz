using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.Tools.Orders;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Application.Orders.Services
{
	internal class OrderService : IOrderService
	{
		private readonly ILogger<OrderService> _logger;

		public OrderService(
			ILogger<OrderService> logger,
			INomenclatureParametersProvider nomenclatureParametersProvider)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));

			PaidDeliveryNomenclatureId = nomenclatureParametersProvider.PaidDeliveryNomenclatureId;
		}

		public int PaidDeliveryNomenclatureId { get; }

		public void UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order)
		{
			OrderItem deliveryPriceItem = order.OrderItems
				.FirstOrDefault(x => x.Nomenclature.Id == PaidDeliveryNomenclatureId);

			#region перенести всё это в OrderStateKey

			bool IsDeliveryForFree = order.SelfDelivery
				|| order.OrderAddressType == OrderAddressType.Service
				|| order.DeliveryPoint.AlwaysFreeDelivery
				|| order.ObservableOrderItems
					.Any(n => n.Nomenclature.Category == NomenclatureCategory.spare_parts)
				|| !order.ObservableOrderItems
					.Any(n => n.Nomenclature.Id != PaidDeliveryNomenclatureId)
				&& (order.BottlesReturn > 0
					|| order.ObservableOrderEquipments.Any()
					|| order.ObservableOrderDepositItems.Any());

			if(IsDeliveryForFree)
			{
				if(deliveryPriceItem != null)
				{
					order.RemoveOrderItem(deliveryPriceItem);
				}
				return;
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

			if(price != 0)
			{
				order.AddOrUpdateDeliveryItem(unitOfWork.GetById<Nomenclature>(PaidDeliveryNomenclatureId), price);

				return;
			}

			if(deliveryPriceItem != null)
			{
				order.RemoveOrderItem(deliveryPriceItem);
			}
		}
	}
}
