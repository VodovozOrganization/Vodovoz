using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts.Requests;
using Microsoft.Extensions.Logging;

namespace FastPaymentEventsSender.Services
{
	/// <inheritdoc/>
	public class MobileAppNotifier : IMobileAppNotifier
	{
		private readonly ILogger<MobileAppNotifier> _logger;
		private readonly HttpClient _httpClient;

		public MobileAppNotifier(
			ILogger<MobileAppNotifier> logger,
			HttpClient httpClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		/// <inheritdoc/>
		public async Task<int> NotifyPaymentStatusChangedAsync(FastPaymentStatusChangeNotificationDto notification, string url)
		{
			if(string.IsNullOrWhiteSpace(url))
			{
				throw new ArgumentException($"'{nameof(url)}' cannot be null or whitespace.", nameof(url));
			}

			_logger.LogInformation(
				"Отправка уведомления о быстрой оплате на мобильное приложение для онлайн заказа {OnlineOrderId}",
				notification.PaymentDetails.OnlineOrderId);

			var content = JsonSerializer.Serialize(notification);
			var response = await _httpClient.PostAsJsonAsync(url, content);

			return (int)response.StatusCode;
		}
	}
}
