using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OrderOnlinePaymentAcceptanceHandler : IOrderOnlinePaymentAcceptanceHandler
	{
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly ISelfDeliveryRepository _selfDeliveryRepository;
		private readonly ICashRepository _cashRepository;
		private readonly IOrderContractUpdater _contractUpdater;

		public OrderOnlinePaymentAcceptanceHandler(
			INomenclatureSettings nomenclatureSettings,
			IRouteListItemRepository routeListItemRepository,
			ISelfDeliveryRepository selfDeliveryRepository,
			ICashRepository cashRepository,
			IOrderContractUpdater contractUpdater)
		{
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_selfDeliveryRepository = selfDeliveryRepository ?? throw new ArgumentNullException(nameof(selfDeliveryRepository));
			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));
		}

		public void AcceptOnlinePayment(
			IUnitOfWork uow,
			IEnumerable<Order> orders,
			int paymentNumber,
			PaymentType paymentType,
			PaymentFrom paymentFrom)
		{
			if(paymentNumber == 0)
			{
				return;
			}
			
			var selfDeliveryOrderPaymentTypes = new[] { PaymentType.Cash, PaymentType.SmsQR };

			foreach(var order in orders)
			{
				if(selfDeliveryOrderPaymentTypes.Contains(order.PaymentType)
					&& order.SelfDelivery
					&& order.OrderStatus == OrderStatus.WaitForPayment
					&& order.PayAfterShipment)
				{
					order.TryCloseSelfDeliveryPayAfterShipmentOrder(
						uow,
						_nomenclatureSettings,
						_routeListItemRepository,
						_selfDeliveryRepository,
						_cashRepository);
					order.IsSelfDeliveryPaid = true;
				}
				
				if(selfDeliveryOrderPaymentTypes.Contains(order.PaymentType)
					&& order.SelfDelivery
					&& order.OrderStatus == OrderStatus.WaitForPayment
					&& !order.PayAfterShipment)
				{
					order.ChangeStatus(OrderStatus.OnLoading);
					order.IsSelfDeliveryPaid = true;
				}
			
				order.OnlinePaymentNumber = paymentNumber;
				order.UpdatePaymentType(paymentType, _contractUpdater);
				order.UpdatePaymentByCardFrom(paymentFrom, _contractUpdater);

				foreach(var routeListItem in _routeListItemRepository.GetRouteListItemsForOrder(uow, order.Id))
				{
					routeListItem.RecalculateTotalCash();
					uow.Save(routeListItem);
				}
			
				uow.Save(order);
			}
		}
	}
}
