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
					title,
					body
				}
			};

			using (HttpResponseMessage response = await _apiClient.PostAsJsonAsync(_sendPushNotificationEndpointURI, request))
			{
				if(!response.IsSuccessStatusCode)
				{
					throw new FCMException(response.ReasonPhrase);
				}
			}
		}

		public async Task SendWakeUpNotification(string pushNotificationClientToken)
		{
			var request = new
			{
				to = pushNotificationClientToken,
				priority = "high"
			};

			//using HttpResponseMessage response = await _apiClient.PostAsJsonAsync(_sendPushNotificationEndpointURI, request);
			//"message": {

			//					"token": recieverFcm,
			//			   "data": {
			//						"title": senderName,
			//   				   "body": message,
			//   				   "chatRoomId": chatRoomId,
			//   				   "sender_profile_pic": senderProfilePic,
			//   				   "senderUid": senderUid,
			//   				   "data_type": messageType,
			//   				   "click_action": "OPEN_CHAT_ROOM"

			// },
			//			   "android": {
			//						"priority": "high"

			// },
			//			   "apns": {
			//						"payload": {
			//							"aps": {
			//								"category": "OPEN_CHAT_ROOM",
			//           				   "sound": "enable",
			//           				   "content-available": 1,
			//       				   },
			//       				   "data": {
			//								"title": senderName,
			//           				   "body": message,
			//           				   "chatRoomId": chatRoomId,
			//           				   "sender_profile_pic": senderProfilePic,
			//           				   "senderUid": senderUid,
			//           				   "data_type": messageType,
			//           				   "click_action": "OPEN_CHAT_ROOM"

			//	 },
			//   				   }
			//					}
			//				}
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
