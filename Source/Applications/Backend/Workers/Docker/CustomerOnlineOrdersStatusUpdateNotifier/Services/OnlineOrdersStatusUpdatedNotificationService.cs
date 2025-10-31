using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CustomerOnlineOrdersStatusUpdateNotifier.Configs;
using CustomerOnlineOrdersStatusUpdateNotifier.Contracts;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Services
{
	public class OnlineOrdersStatusUpdatedNotificationService : IOnlineOrdersStatusUpdatedNotificationService
	{
		private readonly HttpClient _httpClient;
		private readonly NotifierOptions _options;
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private const string _orderIdTemplate = "*номер заказа*";
		private const string _deliveryScheduleFromTemplate = "*интервал доставки*";

		public OnlineOrdersStatusUpdatedNotificationService(
			HttpClient client,
			IOptionsSnapshot<NotifierOptions> options,
			JsonSerializerOptions jsonSerializerOptions)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
			_jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
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

		public string GetPushText(IUnitOfWork unitOfWork, IOnlineOrderStatusUpdatedNotificationRepository notificationRepository,
			ExternalOrderStatus externalOrderStatus, int orderId, TimeSpan deliveryScheduleFrom)
		{
			var onlineOrderNotificationSetting = notificationRepository.GetNotificationSetting(unitOfWork, externalOrderStatus);

			if(onlineOrderNotificationSetting is null)
			{
				throw new InvalidOperationException(
					$"Не найдена настройка уведомления для статуса заказа «{externalOrderStatus}» (orderId={orderId}).");
			}
			
			return onlineOrderNotificationSetting.NotificationText
				.Replace(_orderIdTemplate, orderId.ToString())
				.Replace(_deliveryScheduleFromTemplate, deliveryScheduleFrom.ToString(@"hh\:mm"));
		}
	}
}
