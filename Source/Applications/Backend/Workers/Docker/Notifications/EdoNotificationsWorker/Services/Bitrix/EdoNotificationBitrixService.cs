using EdoNotificationsWorker.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EdoNotificationsWorker.Services.Bitrix
{
	public class EdoNotificationBitrixService : IEdoNotificationBitrixService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IOptionsSnapshot<EdoNotificationsOptions> _options;
		private readonly ILogger<EdoNotificationBitrixService> _logger;

		public EdoNotificationBitrixService(
			IHttpClientFactory httpClientFactory,
			IOptionsSnapshot<EdoNotificationsOptions> options,
			ILogger<EdoNotificationBitrixService> logger)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task NotifyAsync(
			string bitrixDialogs,
			string message,
			CancellationToken cancellationToken = default)
		{
			var dialogIdsList = bitrixDialogs
				.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(e => e.Trim())
				.ToList();

			if(dialogIdsList.Count == 0)
			{
				return;
			}

			using var httpClient = _httpClientFactory.CreateClient();

			foreach(var dialogId in dialogIdsList)
			{
				if(cancellationToken.IsCancellationRequested)
				{
					_logger.LogWarning("Рассылка Bitrix-уведомлений прервана по CancellationToken");
					break;
				}

				await SendToDialogAsync(httpClient, dialogId, message, cancellationToken);
			}
		}

		private async Task SendToDialogAsync(
			HttpClient httpClient,
			string dialogId,
			string message,
			CancellationToken cancellationToken)
		{
			try
			{
				var requestBody = new BitrixRequestDto
				{
					DialogId = dialogId,
					Message = message,
				};

				var json = JsonSerializer.Serialize(requestBody);
				using var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await httpClient.PostAsync(_options.Value.BitrixWebhookUrl, content, cancellationToken);

				response.EnsureSuccessStatusCode();
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Не удалось отправить Bitrix-уведомление для DIALOG_ID {DialogId}",
					dialogId);
			}
		}
	}
}
