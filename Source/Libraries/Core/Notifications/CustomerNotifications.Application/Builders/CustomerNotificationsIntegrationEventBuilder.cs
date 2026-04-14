using CustomerNotifications.Application.Providers;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TransactionalOutbox.Abstractions;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using CustomerNotifications.Contracts;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Application.Builders
{
	public class CustomerNotificationsIntegrationEventBuilder : IIntegrationEventBuilder<CustomerNotificationDomainEvent, CustomerNotificationIntegrationEvent>
	{
		private readonly ICustomerNotificationsSettingsProvider _customerNotificationSettingsProvider;
		private readonly IGenericRepository<RouteListItem> _routeListItemRepository;
		private readonly IGenericRepository<UndeliveredOrder> _undeliveredOrdersRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		// Шаблоны (лучше вынести в отдельный класс-конфиг в будущем)
		private const string _orderIdTemplate = "{OrderId}";
		private const string _deliveryScheduleFromTemplate = "{DeliveryScheduleFrom}";
		private const string _bottlesReturnedTemplate = "{BottlesReturned}";
		private const string _rescheduleDateTemplate = "{RescheduleDate}";
		private const string _rescheduleReasonTemplate = "{RescheduleReason}";

		public CustomerNotificationsIntegrationEventBuilder(
			ICustomerNotificationsSettingsProvider customerNotificationSettingsProvider,
			IGenericRepository<RouteListItem> routeListItemRepository,
			IGenericRepository<UndeliveredOrder> undeliveredOrdersRepository,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			_customerNotificationSettingsProvider = customerNotificationSettingsProvider ?? throw new ArgumentNullException(nameof(customerNotificationSettingsProvider));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_undeliveredOrdersRepository = undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public async Task<CustomerNotificationIntegrationEvent> BuildAsync(
			CustomerNotificationDomainEvent domainEvent,
			CancellationToken cancellationToken = default)
		{
			var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(nameof(CustomerNotificationsIntegrationEventBuilder));

			if(domainEvent == null)
			{
				throw new ArgumentNullException(nameof(domainEvent));
			}

			// Получаем текст из настроек
			var notificationText = _customerNotificationSettingsProvider.GetNotificationText(domainEvent);

			if(string.IsNullOrEmpty(notificationText))
			{
				throw new InvalidOperationException(
					$"Не найдена настройка уведомления для типа события «{domainEvent.CustomerNotificationEventType.GetEnumDisplayName()}» " +
					$"(OnlineOrderId={domainEvent.OnlineOrderId}). (OrderId={domainEvent.OrderId}).");
			}

			OnlineOrder onlineOrder = null;

			if(domainEvent.OnlineOrderId != null)
			{
				onlineOrder = unitOfWork.GetById<OnlineOrder>(domainEvent.OnlineOrderId.Value);

				if(onlineOrder == null)
				{
					throw new InvalidOperationException($"Онлайн заказ с Id {domainEvent.OnlineOrderId} не найден.");
				}
			}

			Order order = null;

			if(domainEvent.OrderId != null)
			{
				order = unitOfWork.GetById<Order>(domainEvent.OrderId.Value);

				if(order == null)
				{
					throw new InvalidOperationException($"Заказ с Id {domainEvent.OrderId} не найден.");
				}
			}

			var deliveryScheduleFrom = onlineOrder?.DeliverySchedule?.DeliveryTime ?? order?.DeliverySchedule?.DeliveryTime;

			notificationText = notificationText
				.Replace(_orderIdTemplate, (domainEvent.OnlineOrderId ?? domainEvent.OrderId).ToString())
				.Replace(_deliveryScheduleFromTemplate, deliveryScheduleFrom ?? "[интервал в заказе не выбран]");

			if(domainEvent.CustomerNotificationEventType == CustomerNotificationEventType.DeliveryCompleted)
			{
				notificationText = await ApplyDeliveryCompletedLogicAsync(unitOfWork, domainEvent, onlineOrder, order, notificationText);
			}
			else if(domainEvent.CustomerNotificationEventType == CustomerNotificationEventType.OrderRescheduled)
			{
				notificationText = await ApplyOrderRescheduledLogicAsync(unitOfWork, domainEvent, onlineOrder, order, notificationText);
			}

			var data = new CustomerNotificationMessage
			{
				CounterpartyErpId = onlineOrder?.CounterpartyId ?? order?.Client?.Id ?? 0,
				Type = _customerNotificationSettingsProvider.GetCustomerPushType(domainEvent),
				Target = _customerNotificationSettingsProvider.GetCustomerPushTarget(domainEvent),
				Title = domainEvent.CustomerNotificationEventType.GetEnumDisplayName(),
				Text = notificationText,
				Params = new Dictionary<string, string>
				{
					["onlineOrderId"] = onlineOrder?.Id.ToString(),
					["orderId"] = order?.Id.ToString()
				}
			};

			var integrationEvent = new CustomerNotificationIntegrationEvent
			{
				Payload = data,
				EventSource = domainEvent.EventSource
			};

			return integrationEvent;
		}

		private async Task<string> ApplyDeliveryCompletedLogicAsync(
			IUnitOfWork unitOfWork,
			CustomerNotificationDomainEvent notificationDomainEvent,
			OnlineOrder onlineOrder,
			Order order,
			string text)
		{
			var undeliveryStatuses = new[]
			{
				OrderStatus.NotDelivered,
				OrderStatus.DeliveryCanceled,
				OrderStatus.Canceled
			};

			var currentOrder = onlineOrder.Orders
				.OrderByDescending(o => o.CreateDate)
				.FirstOrDefault(o => !undeliveryStatuses.Contains(o.OrderStatus))
				?? order;

			if(currentOrder == null)
			{
				throw new InvalidOperationException(
					$"Не найден текущий заказ для DeliveryCompleted (OnlineOrderId={notificationDomainEvent.OnlineOrderId}).");
			}

			var address = _routeListItemRepository.Get(
					unitOfWork,
					x => x.Status != RouteListItemStatus.Transfered && x.Order.Id == currentOrder.Id)
				.SingleOrDefault();

			var bottlesInfo = currentOrder.GetTotalWater19LCount() > 0
				? $"Вы сдали {address?.BottlesReturned ?? 0} бутылей"
				: string.Empty;

			return text.Replace(_bottlesReturnedTemplate, bottlesInfo);
		}

		private async Task<string> ApplyOrderRescheduledLogicAsync(
			IUnitOfWork unitOfWork,
			CustomerNotificationDomainEvent notificationDomainEvent,
			OnlineOrder onlineOrder,
			Order order,
			string text)
		{
			var undeliveryStatuses = new[]
			{
				OrderStatus.NotDelivered,
				OrderStatus.DeliveryCanceled,
				OrderStatus.Canceled
			};

			var undeliveredOrderId = onlineOrder.Orders
				.OrderByDescending(o => o.CreateDate)
				.FirstOrDefault(o => undeliveryStatuses.Contains(o.OrderStatus))
				?.Id;

			var currentOrder = onlineOrder.Orders
				.OrderByDescending(o => o.CreateDate)
				.FirstOrDefault(o => !undeliveryStatuses.Contains(o.OrderStatus))
				?? order;

			if(currentOrder == null)
			{
				throw new InvalidOperationException(
					$"Не найден текущий заказ для OrderRescheduled (OnlineOrderId={onlineOrder.Id}, OrderId = {order.Id}).");
			}

			var undelivery = _undeliveredOrdersRepository.Get(
					unitOfWork,
					x => x.OldOrder.Id == undeliveredOrderId)
				.OrderByDescending(o => o.TimeOfCreation)
				.FirstOrDefault();

			var rescheduleReason = undelivery?.UndeliveryDetalization?.CustomerNotificationText ?? string.Empty;

			return text
				.Replace(_rescheduleDateTemplate, currentOrder.DeliveryDate?.ToString("D") ?? "")
				.Replace(_rescheduleReasonTemplate, rescheduleReason);
		}
	}
}
