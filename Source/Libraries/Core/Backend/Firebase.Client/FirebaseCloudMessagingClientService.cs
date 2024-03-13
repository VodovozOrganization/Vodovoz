using Firebase.Client.Exceptions;
using FirebaseCloudMessaging.Client.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Vodovoz.FirebaseCloudMessaging.Client.Requests;
using Vodovoz.FirebaseCloudMessaging.Client.Responses;

namespace FirebaseCloudMessaging.Client
{
	internal class FirebaseCloudMessagingClientService : IFirebaseCloudMessagingClientService
	{
		private readonly ILogger<FirebaseCloudMessagingClientService> _logger;
		private readonly IOptions<FirebaseCloudMessagingSettings> _settings;
		private readonly HttpClient _httpClient;

		public FirebaseCloudMessagingClientService(
			ILogger<FirebaseCloudMessagingClientService> logger,
			IOptions<FirebaseCloudMessagingSettings> settings,
			HttpClient httpClient)
		{
			_logger = logger;
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		public async Task Authenticate()
		{
			var authenticationUrl = $"{_settings.Value.ApiBase}/auth/firebase.messaging";

			await _httpClient.PostAsJsonAsync(authenticationUrl, new AuthorizationRequest());
		}

		public async Task SendMessage(string recipientToken, string title, string body, object data = null)
		{
			await SendMessage(new MessageRequest
			{

			});
		}

		private async Task SendMessage(MessageRequest sendMessageRequest)
		{
			try
			{
				var endpointUrl = $"{_settings.Value.ApiBase}/v1/projects/{_settings.Value.ApplicationId}/messages:send";

				var result = await _httpClient.PostAsJsonAsync(endpointUrl, sendMessageRequest);

				if(result.IsSuccessStatusCode)
				{
					var response = await result.Content.ReadAsAsync<MessageResponse>();
					
					if(response != null)
					{
						_logger.LogInformation("Сообщение успешно доставлено: {MessageName}", response.Name);
					}
					else
					{
						_logger.LogWarning("Firebase Cloud Messaging Api ответило успешным кодом, но получить данные ответа не удалось");
					}

					return;
				}
				else
				{
					throw new FirebaseCloudMessagingClientServiceException(result.StatusCode, result.ReasonPhrase);
				}
			}
			catch(Exception ex)
			{
				throw new FirebaseCloudMessagingClientServiceException("Ошибка при отправке запроса в Firebase", ex);
			}
		}
	}

	internal class AuthorizationRequest
	{
	}
}
