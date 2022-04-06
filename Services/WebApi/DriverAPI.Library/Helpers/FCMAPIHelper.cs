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

		public async Task SendPushNotification(string pushNotificationClientToken, string title, string body)
		{
			var request = new
			{
				to = pushNotificationClientToken,
				notification = new
				{
					title = title,
					body = body,
					sound = "default"
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
			_apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + apiConfiguration["AccessToken"]);

			_sendPushNotificationEndpointURI = apiConfiguration["SendPushNotificationEndpointURI"];
		}
	}
}
