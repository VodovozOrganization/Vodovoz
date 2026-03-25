using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FastPaymentEventsSender.Services;
using FastPaymentsApi.Contracts.Requests;
using Microsoft.Extensions.Logging;
using Vodovoz.Settings.FastPayments;

namespace FastPaymentEventsSender.ApiClients
{
	/// <inheritdoc/>
	public class WebSiteClient : IWebSiteClient
	{
		private readonly ILogger<WebSiteClient> _logger;
		private readonly ISiteSettings _siteSettings;
		private readonly HttpClient _httpClient;

		public WebSiteClient(
			ILogger<WebSiteClient> logger,
			HttpClient httpClient,
			ISiteSettings siteSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_siteSettings = siteSettings ?? throw new ArgumentNullException(nameof(siteSettings));
		}

		/// <inheritdoc/>
		public async Task<int> NotifyPaymentStatusChangedAsync(FastPaymentStatusChangeNotificationDto notification)
		{
			var uri = _siteSettings.NotifyOfFastPaymentStatusChangedUri;
			var content = JsonSerializer.Serialize(notification);
			var json = JsonContent.Create(notification);
			
			_logger.LogInformation(
				"Отправка уведомления о быстрой оплате на сайт для онлайн заказа {OnlineOrderId} {Notification}",
				notification.PaymentDetails.OnlineOrderId,
				content);
			
			var response = await _httpClient.PostAsync(uri, json);
			var responseString =  await response.Content.ReadAsStringAsync();
			
			_logger.LogInformation("Ответ по {OnlineOrderId}: {NotificationResponse}",
				notification.PaymentDetails.OnlineOrderId,
				responseString);

			return (int)response.StatusCode;
		}
	}
}
