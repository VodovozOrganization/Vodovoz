using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DriverAPI.Library.Helpers
{
	public class FCMAPIHelper : IFCMAPIHelper
	{
		private string sendPushNotificationEndpointURI;
		private HttpClient _apiClient;

		public FCMAPIHelper(IConfiguration configuration)
		{
			InitializeClient(configuration);
		}

		public async Task SendPushNotification(string pushNotificationClientToken, string sender, string message)
		{
			var request = new FCMSendPushRequestModel()
			{
				to = pushNotificationClientToken,
				data = new FCMSendPushMessageModel()
				{
					notificationType = "orderPaymentStatusChange",
					sender = sender,
					message = message
				}
			};

			using (HttpResponseMessage response = await _apiClient.PostAsJsonAsync(sendPushNotificationEndpointURI, request))
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
			_apiClient.DefaultRequestHeaders.Add("Authorization", $"key={apiConfiguration["AccessToken"]}");
			_apiClient.DefaultRequestHeaders.Add("Sender", $"id={apiConfiguration["AppId"]}");

			sendPushNotificationEndpointURI = apiConfiguration["SendPushNotificationEndpointURI"];
		}
	}
}
