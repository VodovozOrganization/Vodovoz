using CustomerNotifications.Consumer.Contracts;
using CustomerNotifications.Consumer.Options;
using CustomerNotifications.Contracts.Messages;
using CustomerNotifications.Contracts.Providers;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Extensions;

namespace CustomerNotifications.Consumer.Services
{
	public class OnlineOrdersStatusUpdatedNotificationService : IOnlineOrdersStatusUpdatedNotificationService
	{
		private readonly HttpClient _httpClient;
		private readonly NotifierOptions _options;
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private const string _orderIdTemplate = "*номер заказа*";
		private const string _deliveryScheduleFromTemplate = "*интервал доставки*";
		private const string _bottlesReturnedTemplate = "*Вы сдали количество пустых бутылей*";
		private const string _rescheduleDateTemplate = "*дата переноса*";
		private const string _rescheduleReasonTemplate = "*причина переноса*";


		public OnlineOrdersStatusUpdatedNotificationService(
			HttpClient client,
			IOptionsSnapshot<NotifierOptions> options,
			JsonSerializerOptions jsonSerializerOptions,
			IRouteListItemRepository routeListItemRepository,
			IOrderRepository orderRepository,
			IUndeliveredOrdersRepository undeliveredOrdersRepository
			)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
			_jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_undeliveredOrdersRepository = undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
		}

		public async Task<int> NotifyOfOnlineOrderStatusUpdatedAsync(
			OnlineOrderStatusUpdatedDto statusUpdatedDto, Source source, CancellationToken cancellationToken = default)
		{
			var content = JsonContent.Create(statusUpdatedDto, mediaType: null, _jsonSerializerOptions);
			var response = await _httpClient.PutAsync(GetUriString(source), content, cancellationToken);
			return (int)response.StatusCode;
		}

		private string GetUriString(Source source)
		{
			switch(source)
			{
				case Source.MobileApp:
					return $"{_options.MobileAppUriOptions.BaseUrl}{_options.MobileAppUriOptions.NotificationAddress}";
				case Source.VodovozWebSite:
					return $"{_options.VodovozWebSiteUriOptions.BaseUrl}{_options.VodovozWebSiteUriOptions.NotificationAddress}";
				default:
					throw new ArgumentOutOfRangeException(nameof(Source), source, null);
			}
		}

		public string GetPushText(
			IUnitOfWork unitOfWork,
			IOnlineOrderNotificationSettingsProvider onlineOrderNotificationSettingsProvider,
			CustomerNotificationMessage message,
			OnlineOrder onlineOrder)
		{
			var onlineOrderNotificationText = onlineOrderNotificationSettingsProvider.GetNotificationText(message.CustomerNotificationEventType);

			if(string.IsNullOrEmpty(onlineOrderNotificationText))
			{
				throw new InvalidOperationException(
					$"Не найдена настройка уведомления для типа события «{message.CustomerNotificationEventType.GetEnumDisplayName()}» (OnlineOrderId={message.OnlineOrderId}).");
			}

			onlineOrderNotificationText = onlineOrderNotificationText
				.Replace(_orderIdTemplate, message.OnlineOrderId.ToString())
				.Replace(_deliveryScheduleFromTemplate, onlineOrder.DeliverySchedule?.DeliveryTime ?? "[интервал в заказе не выбран]");

			var undeliveryStatus = _orderRepository.GetUndeliveryStatuses();		

			if(message.CustomerNotificationEventType == CustomerNotificationEventType.DeliveryCompleted)
			{
				var currentOrder = onlineOrder.Orders.FirstOrDefault(o => !undeliveryStatus.Contains(o.OrderStatus));

				if(currentOrder == null)
				{
					throw new InvalidOperationException(
						$"Не найден текущий заказ «{message.CustomerNotificationEventType.GetEnumDisplayName()}» (OnlineOrderId={message.OnlineOrderId}).");
				}

				var address = _routeListItemRepository.GetRouteListItemForOrder(unitOfWork, currentOrder);
				onlineOrderNotificationText = onlineOrderNotificationText
					.Replace(_bottlesReturnedTemplate,
						currentOrder.GetTotalWater19LCount() > 0
							? $"Вы сдали {address?.BottlesReturned.ToString() ?? "[не найден адрес]"} бутылей"
							: string.Empty);
			}

			if(message.CustomerNotificationEventType == CustomerNotificationEventType.OrderRescheduled)
			{
				var undeliveredOrder = onlineOrder.Orders
					.OrderByDescending(o=>o.CreateDate)
					.FirstOrDefault(o => undeliveryStatus.Contains(o.OrderStatus));

				var currentOrder = onlineOrder.Orders
					.OrderByDescending(o => o.CreateDate)
					.FirstOrDefault(o => !undeliveryStatus.Contains(o.OrderStatus));

				var undelivery = _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(unitOfWork, undeliveredOrder)
					.OrderByDescending(o => o.TimeOfCreation)
					.FirstOrDefault();

				var rescheduleReason = undelivery?.UndeliveryDetalization?.CustomerNotificationText;

				onlineOrderNotificationText
					.Replace(_rescheduleDateTemplate, currentOrder.DeliveryDate?.ToString("D"))
					.Replace(_rescheduleReasonTemplate, rescheduleReason);
			}

			return onlineOrderNotificationText;
		}
	}
}
