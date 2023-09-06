using Firebase.Client.Exceptions;
using FirebaseCloudMessaging.Client.Options;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FirebaseCloudMessaging.Client
{
	internal class FirebaseCloudMessagingService : IFirebaseCloudMessagingService
	{
		private readonly IOptions<FirebaseCloudMessagingSettings> _settings;
		private readonly HttpClient _httpClient;

		public FirebaseCloudMessagingService(
			IOptions<FirebaseCloudMessagingSettings> settings,
			HttpClient httpClient)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		public async Task SendMessage(string pushNotificationClientToken, string title, string body)
		{
			var request = new
			{
				to = pushNotificationClientToken,
				notification = new
				{
					title,
					body
				}
			};

			await SendMessage(request);
		}

		public async Task SendFastDeliveryAddressCanceledMessage(string pushNotificationClientToken, int orderId)
		{
			await SendMessage(
				pushNotificationClientToken,
				"Отмена заказа с доставкой за час",
				$"Заказ №{orderId} с доставкой за час отменен");
		}

		public Task SendWakeUpMessage(string pushNotificationClientToken)
		{
			throw new NotImplementedException();
		}

		internal async Task SendMessage<T>(T request)
		{
			try
			{
				var result = await _httpClient.PostAsJsonAsync(_settings.Value.SendMessageEndpointURI, request);

				if(result.IsSuccessStatusCode)
				{
					return;
				}
				else
				{
					throw new FirebaseServiceException(result.StatusCode, result.ReasonPhrase);
				}
			}
			catch(Exception ex)
			{
				throw new FirebaseServiceException("Ошибка при отправке запроса в Firebase", ex);
			}
		}
	}
}
