using DriverAPI.Library.DTOs;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DriverAPI.Library.Helpers
{
	public class FCMAPIHelper : IFCMAPIHelper
	{
		private string _sendPushNotificationEndpointURI;
		private HttpClient _apiClient;

		public FCMAPIHelper(IConfiguration configuration)
		{
			InitializeClient(configuration);
		}

		public async Task SendPushNotification(string pushNotificationClientToken, string sender, string message)
		{
			var request = new FCMSendPushRequestDto()
			{
				to = pushNotificationClientToken,
				data = new FCMSendPushMessageDto()
				{
					notificationType = "orderPaymentStatusChange",
					sender = sender,
					message = message
				}
			};

			using (HttpResponseMessage response = await _apiClient.PostAsJsonAsync(_sendPushNotificationEndpointURI, request))
			{
				if (response.IsSuccessStatusCode)
				{
					return;
				}
				else
				{
					throw new FCMException(response.ReasonPhrase);
				}
			}
		}

		private void InitializeClient(IConfiguration configuration)
		{
			var apiConfiguration = configuration.GetSection("FCMAPI");

			_apiClient = new HttpClient();
			_apiClient.BaseAddress = new Uri(apiConfiguration["ApiBase"]);
			_apiClient.DefaultRequestHeaders.Accept.Clear();
			_apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_apiClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={apiConfiguration["AccessToken"]}");
			_apiClient.DefaultRequestHeaders.TryAddWithoutValidation("Sender", $"id={apiConfiguration["AppId"]}");

			_sendPushNotificationEndpointURI = apiConfiguration["SendPushNotificationEndpointURI"];
		}
	}
}
