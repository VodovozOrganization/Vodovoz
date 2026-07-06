using System;
using System.Collections.Generic;
using System.Linq;
using CustomerNotifications.Contracts;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class OrderOnlinePaymentAcceptanceHandler : IOrderOnlinePaymentAcceptanceHandler
	{
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly ISelfDeliveryRepository _selfDeliveryRepository;
		private readonly ICashRepository _cashRepository;
		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly IOrderContractUpdater _contractUpdater;
		private readonly IOutboxNotificationPublisher<CustomerNotificationDomainEvent> _customerNotificationPublisher;

		public OrderOnlinePaymentAcceptanceHandler(
			INomenclatureSettings nomenclatureSettings,
			IRouteListItemRepository routeListItemRepository,
			ISelfDeliveryRepository selfDeliveryRepository,
			ICashRepository cashRepository,
			IOutboxNotificationPublisher<CustomerNotificationDomainEvent> customerNotificationPublisher,
			IFastPaymentRepository fastPaymentRepository,
			IOrderContractUpdater contractUpdater)
		{
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_selfDeliveryRepository = selfDeliveryRepository ?? throw new ArgumentNullException(nameof(selfDeliveryRepository));
			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));
			_customerNotificationPublisher = customerNotificationPublisher ?? throw new ArgumentNullException(nameof(customerNotificationPublisher));
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
					var customerNotificationEvent = new CustomerNotificationDomainEvent(CustomerNotificationEventType.CourierAssigned, onlineOrderId: order.OnlineOrder?.Id, orderId: order.Id);
					_customerNotificationPublisher.TryPublish(uow, customerNotificationEvent);
				}

				//Проверяем два дня, текущий и прошлый, если платеж создали ночью на стыке дней, а оплатили после
				var today = DateTime.Today;
				var fastPayment = _fastPaymentRepository.GetPerformedFastPaymentByExternalId(uow, paymentNumber, today)
					?? _fastPaymentRepository.GetPerformedFastPaymentByExternalId(uow, paymentNumber, today.AddDays(-1));
				order.OnlinePaymentNumber = paymentNumber;

				if(fastPayment is null)
				{
					order.UpdatePaymentType(paymentType, _contractUpdater);
					order.UpdatePaymentByCardFrom(paymentFrom, _contractUpdater);
				}
				else
				{
					order.UpdatePaymentType(paymentType, _contractUpdater, false);
					order.UpdatePaymentByCardFrom(paymentFrom, _contractUpdater, false);
					_contractUpdater.ForceUpdateContract(uow, order, fastPayment.Organization);
				}

				foreach(var routeListItem in _routeListItemRepository.GetRouteListItemsForOrder(uow, order.Id))
				{
					routeListItem.RecalculateTotalCash();
					uow.Save(routeListItem);
				}
			
				uow.Save(order);
			}
		}
		
		public void AcceptOnlinePayment(
			IUnitOfWork uow,
			Vodovoz.Domain.FastPayments.FastPayment fastPayment)
		{
			var order = fastPayment.Order;

			if(order is null)
			{
				return;
			}
			
			var selfDeliveryOrderPaymentTypes = new[] { PaymentType.Cash, PaymentType.SmsQR };

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
				var customerNotificationEvent = new CustomerNotificationDomainEvent(CustomerNotificationEventType.CourierAssigned, onlineOrderId: order.OnlineOrder?.Id, orderId: order.Id);
				_customerNotificationPublisher.TryPublish(uow, customerNotificationEvent);
			}

			order.OnlinePaymentNumber = fastPayment.ExternalId;
			order.UpdatePaymentType(fastPayment.PaymentType, _contractUpdater, false);
			order.UpdatePaymentByCardFrom(fastPayment.PaymentByCardFrom, _contractUpdater, false);
			_contractUpdater.ForceUpdateContract(uow, order, fastPayment.Organization);

			foreach(var routeListItem in _routeListItemRepository.GetRouteListItemsForOrder(uow, order.Id))
			{
				routeListItem.RecalculateTotalCash();
				uow.Save(routeListItem);
			}
		
			uow.Save(order);
		}
	}
}
