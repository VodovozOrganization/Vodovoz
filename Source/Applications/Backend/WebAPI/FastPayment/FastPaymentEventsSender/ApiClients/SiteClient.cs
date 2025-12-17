using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FastPaymentEventsSender.Services;
using FastPaymentsApi.Contracts.Requests;
using Microsoft.Extensions.Logging;
using Vodovoz.Settings.FastPayments;

namespace FastPaymentEventsSender.ApiClients
{
	/// <inheritdoc/>
	public class SiteClient : IWebSiteNotifier
	{
		private readonly ILogger<SiteClient> _logger;
		private readonly ISiteSettings _siteSettings;
		private readonly HttpClient _httpClient;

		public SiteClient(
			ILogger<SiteClient> logger,
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
			_logger.LogInformation(
				"Отправка уведомления о быстрой оплате на сайт для онлайн заказа {OnlineOrderId}",
				notification.PaymentDetails.OnlineOrderId);
			
			var uri = _siteSettings.NotifyOfFastPaymentStatusChangedUri;
			var content = JsonSerializer.Serialize(notification);
			var response = await _httpClient.PostAsJsonAsync(uri, content);

			return (int)response.StatusCode;
		}
	}
}
