using Vodovoz.Core.Domain.Clients;
using CustomerNotificationsWorker.Config;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TransactionalOutbox.Serialization;
using CustomerNotifications.Contracts;

namespace CustomerNotificationsWorker
{
	public class CustomerNotificationsConsumer : IConsumer<CustomerNotificationIntegrationEvent>
	{
		private readonly IOptionsSnapshot<NotifierOptions> _options;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly int _httpClientTimeoutInSeconds;
		private readonly ILogger<CustomerNotificationsConsumer> _logger;

		public CustomerNotificationsConsumer(
			IOptionsSnapshot<NotifierOptions> options,
			IHttpClientFactory httpClientFactory,
			ILogger<CustomerNotificationsConsumer> logger)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_httpClientTimeoutInSeconds = options.Value.SendingTimeoutInSeconds;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<CustomerNotificationIntegrationEvent> context)
		{
			var message = context.Message;
			var attempt = context.GetRetryCount() + 1;

			_logger.LogInformation(
				"Попытка #{Attempt}: Отправка push-уведомления. CounterpartyErpId: {CounterpartyErpId}",
				attempt, message.Payload?.CounterpartyErpId);

			await NotifyCustomerAsync(message, context.CancellationToken);

			_logger.LogInformation(
				"Попытка #{Attempt} завершена успешно.  CounterpartyErpId: {CounterpartyErpId}",
				attempt, message.Payload?.CounterpartyErpId);
		}

		private async Task NotifyCustomerAsync(CustomerNotificationIntegrationEvent customerEvent, CancellationToken cancellationToken)
		{
			if(customerEvent is null)
			{ 
				throw new ArgumentNullException(nameof(customerEvent), "Источник уведомления не может быть null.");
			}

			var httpClient = _httpClientFactory.CreateClient();

			httpClient.Timeout = TimeSpan.FromSeconds(_httpClientTimeoutInSeconds);

			var content = JsonContent.Create(				
				customerEvent.Payload,
				mediaType: null,
				OutboxJsonSerializerOptions.Instance);

			var response = await httpClient.PutAsync(
				GetUriString(customerEvent.EventSource.Value),
				content,
				cancellationToken);

			if(!response.IsSuccessStatusCode)
			{
				var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

				_logger.LogWarning("Неуспешный ответ от сервиса уведомлений. Status: {StatusCode}, Body: {ResponseBody}",
					response.StatusCode, responseBody);

				response.EnsureSuccessStatusCode();
			}
		}

		private string GetUriString(Source source)
		{
			string baseUrl;
			string address;

			switch(source)
			{
				case Source.MobileApp:
					baseUrl = _options.Value.MobileAppUriOptions.BaseUrl;
					address = _options.Value.MobileAppUriOptions.NotificationAddress;
					break;

				case Source.VodovozWebSite:
					baseUrl = _options.Value.VodovozWebSiteUriOptions.BaseUrl;
					address = _options.Value.VodovozWebSiteUriOptions.NotificationAddress;
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестный источник push-уведомлений");
			}

			if(string.IsNullOrWhiteSpace(baseUrl))
			{
				throw new InvalidOperationException($"BaseUrl для источника {source} не настроен или пустой.");
			}

			baseUrl = baseUrl.TrimEnd('/');
			address = address?.TrimStart('/') ?? "";

			return $"{baseUrl}/{address}";
		}
	}
}
