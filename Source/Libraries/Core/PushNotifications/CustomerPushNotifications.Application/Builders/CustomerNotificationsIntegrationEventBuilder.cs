using CustomerPushNotifications.Application.Providers;
using CustomerPushNotifications.Contracts;
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

namespace CustomerPushNotifications.Application.Builders
{
	public class CustomerNotificationsIntegrationEventBuilder : IIntegrationEventBuilder<CustomerNotificationDomainEvent, CustomerNotificationIntegrationEvent>
	{

		private readonly ICustomerPushNotificationsSettingsProvider _customerNotificationSettingsProvider;
		private readonly IGenericRepository<RouteListItem> _routeListItemRepository;
		private readonly IGenericRepository<UndeliveredOrder> _undeliveredOrdersRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		// Шаблоны (лучше вынести в отдельный класс-конфиг в будущем)
		private const string OrderIdTemplate = "{OrderId}";
		private const string DeliveryScheduleFromTemplate = "{DeliveryScheduleFrom}";
		private const string BottlesReturnedTemplate = "{BottlesReturned}";
		private const string RescheduleDateTemplate = "{RescheduleDate}";
		private const string RescheduleReasonTemplate = "{RescheduleReason}";

		public CustomerNotificationsIntegrationEventBuilder(
			ICustomerPushNotificationsSettingsProvider customerNotificationSettingsProvider,
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
					$"(OnlineOrderId={domainEvent.OnlineOrderId}).");
			}

			// Загружаем OnlineOrder
			var onlineOrder = unitOfWork.GetById<OnlineOrder>(domainEvent.OnlineOrderId);

			if(onlineOrder == null)
			{
				throw new InvalidOperationException($"Онлайн заказ с Id {domainEvent.OnlineOrderId} не найден.");
			}

			// Применяем базовые замены
			notificationText = notificationText
				.Replace(OrderIdTemplate, domainEvent.OnlineOrderId.ToString())
				.Replace(DeliveryScheduleFromTemplate, onlineOrder.DeliverySchedule?.DeliveryTime ?? "[интервал в заказе не выбран]");

			// Специфичная логика для разных типов событий
			if(domainEvent.CustomerNotificationEventType == CustomerNotificationEventType.DeliveryCompleted)
			{
				notificationText = await ApplyDeliveryCompletedLogicAsync(unitOfWork, domainEvent, onlineOrder, notificationText);
			}
			else if(domainEvent.CustomerNotificationEventType == CustomerNotificationEventType.OrderRescheduled)
			{
				notificationText = await ApplyOrderRescheduledLogicAsync(unitOfWork, domainEvent, onlineOrder, notificationText);
			}

			// Формируем финальное сообщение`
			var data = new CustomerNotificationMessage
			{
				CounterpartyErpId = onlineOrder.CounterpartyId,
				Type = _customerNotificationSettingsProvider.GetCustomerPushType(domainEvent),
				Target = _customerNotificationSettingsProvider.GetCustomerPushTarget(domainEvent),
				Title = domainEvent.CustomerNotificationEventType.GetEnumDisplayName(),
				Text = notificationText,
				Params = new Dictionary<string, string>
				{
					["onlineOrderId"] = onlineOrder.Id.ToString()
				}
			};

			var integrationEvent = new CustomerNotificationIntegrationEvent
			{
				Data = data
			};

			return integrationEvent;
		}

		private async Task<string> ApplyDeliveryCompletedLogicAsync(
			IUnitOfWork unitOfWork,
			CustomerNotificationDomainEvent notificationDomainEvent,
			OnlineOrder onlineOrder,
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
				.FirstOrDefault(o => !undeliveryStatuses.Contains(o.OrderStatus));

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

			return text.Replace(BottlesReturnedTemplate, bottlesInfo);
		}

		private async Task<string> ApplyOrderRescheduledLogicAsync(
			IUnitOfWork unitOfWork,
			CustomerNotificationDomainEvent notificationDomainEvent,
			OnlineOrder onlineOrder,
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
				?.Id ?? 0;

			var currentOrder = onlineOrder.Orders
				.OrderByDescending(o => o.CreateDate)
				.FirstOrDefault(o => !undeliveryStatuses.Contains(o.OrderStatus));

			if(currentOrder == null)
			{
				throw new InvalidOperationException(
					$"Не найден текущий заказ для OrderRescheduled (OnlineOrderId={notificationDomainEvent.OnlineOrderId}).");
			}

			var undelivery = _undeliveredOrdersRepository.Get(
					unitOfWork,
					x => x.OldOrder.Id == undeliveredOrderId)
				.OrderByDescending(o => o.TimeOfCreation)
				.FirstOrDefault();

			var rescheduleReason = undelivery?.UndeliveryDetalization?.CustomerNotificationText ?? string.Empty;

			return text
				.Replace(RescheduleDateTemplate, currentOrder.DeliveryDate?.ToString("D") ?? "")
				.Replace(RescheduleReasonTemplate, rescheduleReason);
		}
	}
}
