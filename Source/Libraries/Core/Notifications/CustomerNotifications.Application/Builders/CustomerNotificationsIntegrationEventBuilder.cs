using CustomerNotifications.Application.Providers;
using CustomerNotifications.Application.Templates;
using CustomerNotifications.Contracts;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TransactionalOutbox.Abstractions;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using VodovozBusiness.Extensions;

namespace CustomerNotifications.Application.Builders
{
	public class CustomerNotificationsIntegrationEventBuilder : IIntegrationEventBuilder<CustomerNotificationDomainEvent, CustomerNotificationIntegrationEvent>
	{
		private readonly ICustomerNotificationsSettingsProvider _customerNotificationSettingsProvider;
		private readonly IGenericRepository<RouteListItem> _routeListItemRepository;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public CustomerNotificationsIntegrationEventBuilder(
			ICustomerNotificationsSettingsProvider customerNotificationSettingsProvider,
			IGenericRepository<RouteListItem> routeListItemRepository,
			IGenericRepository<UndeliveredOrder> undeliveredOrdersRepository,
			IGenericRepository<Order> orderRepository,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			_customerNotificationSettingsProvider = customerNotificationSettingsProvider ?? throw new ArgumentNullException(nameof(customerNotificationSettingsProvider));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public async Task<CustomerNotificationIntegrationEvent> BuildAsync(
			CustomerNotificationDomainEvent domainEvent,
			CancellationToken cancellationToken = default)
		{
			if(domainEvent == null)
			{
				throw new ArgumentNullException(nameof(domainEvent));
			}

			var notificationText = _customerNotificationSettingsProvider.GetNotificationText(domainEvent);

			if(string.IsNullOrEmpty(notificationText))
			{
				throw new InvalidOperationException(
					$"Не найдена настройка уведомления для типа события «{domainEvent.CustomerNotificationEventType.GetEnumDisplayName()}» " +
					$"(OnlineOrderId={domainEvent.OnlineOrderId}). (OrderId={domainEvent.OrderId}).");
			}

			OnlineOrder onlineOrder = null;
			Order order = null;

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(nameof(CustomerNotificationsIntegrationEventBuilder)))
			{
				if(domainEvent.OnlineOrderId != null)
				{
					onlineOrder = unitOfWork.GetById<OnlineOrder>(domainEvent.OnlineOrderId.Value);

					if(onlineOrder == null)
					{
						throw new InvalidOperationException($"Онлайн заказ с Id {domainEvent.OnlineOrderId} не найден.");
					}
				}

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
					.Replace(NotificationTemplates.OrderId, (domainEvent.OnlineOrderId ?? domainEvent.OrderId).ToString())
					.Replace(NotificationTemplates.DeliveryScheduleFrom, deliveryScheduleFrom ?? "[интервал в заказе не выбран]");

				if(domainEvent.CustomerNotificationEventType == CustomerNotificationEventType.DeliveryCompleted)
				{
					notificationText = await ApplyDeliveryCompletedAsync(unitOfWork, domainEvent, onlineOrder, order, notificationText, cancellationToken);
				}
				else if(domainEvent.CustomerNotificationEventType == CustomerNotificationEventType.OrderRescheduled)
				{
					notificationText = await ApplyOrderRescheduledAsync(unitOfWork, domainEvent, notificationText, cancellationToken);
				}

				var integrationEvent = new CustomerNotificationIntegrationEvent
				{
					EventSource = domainEvent.EventSource
				};

				// Временный костыль для сайта
				if(domainEvent.EventSource == Source.VodovozWebSite)
				{
					if(onlineOrder != null)
					{
						var webSiteData = new WebSiteMessage
						{
							ExternalOrderId = onlineOrder.ExternalOrderId,
							OnlineOrderId = onlineOrder.Id,
							DeliveryDate = onlineOrder.OnlineOrderStatus != OnlineOrderStatus.Canceled ? onlineOrder.DeliveryDate : (DateTime?)null,
							DeliveryScheduleId = onlineOrder.OnlineOrderStatus != OnlineOrderStatus.Canceled ? onlineOrder.DeliveryScheduleId : null,
							OrderStatus = onlineOrder.GetExternalOrderStatus(),
							PushText = notificationText
						};

						integrationEvent.WebSitePayload = webSiteData;
					}
				}
				else
				{
					var moblieAppData = new CustomerNotificationMessage
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

					integrationEvent.Payload = moblieAppData;
				}

				return integrationEvent;
			}
		}

		private async Task<string> ApplyDeliveryCompletedAsync(
			IUnitOfWork unitOfWork,
			CustomerNotificationDomainEvent notificationDomainEvent,
			OnlineOrder onlineOrder,
			Order order,
			string text,
			CancellationToken cancellationToken)
		{
			var currentOrder = onlineOrder?.Orders
				?.FirstOrDefault()
				?? order;

			var address =
				(await _routeListItemRepository.GetAsync(
					unitOfWork,
					x => x.Status != RouteListItemStatus.Transfered && x.Order.Id == currentOrder.Id,
					cancellationToken: cancellationToken))
				.Value
				.Single();

			var bottlesInfo = currentOrder.GetTotalWater19LCount() > 0
				? $"Вы сдали {address?.BottlesReturned ?? 0} пустых бутылей."
				: string.Empty;

			return text.Replace(NotificationTemplates.BottlesReturned, bottlesInfo);
		}

		private async Task<string> ApplyOrderRescheduledAsync(
			IUnitOfWork unitOfWork,
			CustomerNotificationDomainEvent domainEvent,
			string text,
			CancellationToken cancellationToken)
		{
			if(domainEvent.RescheduledNewOrderId is null)
			{
				throw new ArgumentNullException($"Для события {domainEvent.CustomerNotificationEventType} должен быть заполнен идентификатор перенесенного заказа: {nameof(domainEvent.RescheduledNewOrderId)}.");
			}

			var rescheduledOrder =
				   (await _orderRepository.GetAsync(
					   unitOfWork,
					   x => x.Id == domainEvent.RescheduledNewOrderId,
					   cancellationToken: cancellationToken))
				   .Value
				   .Single();

			return text
				.Replace(NotificationTemplates.RescheduleDate, rescheduledOrder.DeliveryDate?.ToString("D") ?? "")
				.Replace(NotificationTemplates.RescheduleReason, domainEvent.UndeliveryCustomerMessage);
		}
	}
}
