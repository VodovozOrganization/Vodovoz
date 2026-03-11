using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FastPaymentEventsSender.Services;
using FastPaymentsApi.Contracts.Requests;
using Microsoft.Extensions.Logging;

namespace FastPaymentEventsSender.ApiClients
{
	/// <inheritdoc/>
	public class AiBotClient : IAiBotClient
	{
		private readonly ILogger<AiBotClient> _logger;
		private readonly HttpClient _httpClient;

		public AiBotClient(
			ILogger<AiBotClient> logger,
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
				"Отправка уведомления о быстрой оплате для ИИ бота по онлайн заказу {OnlineOrderId}",
				notification.PaymentDetails.OnlineOrderId);

			var response = await _httpClient.PostAsJsonAsync(url, notification);

			return (int)response.StatusCode;
		}
	}
}
