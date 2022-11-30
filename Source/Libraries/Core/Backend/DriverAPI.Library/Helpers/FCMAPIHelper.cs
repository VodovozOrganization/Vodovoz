using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace DriverAPI.Library.Helpers
{
	public class FCMAPIHelper : IFCMAPIHelper
	{
		private readonly string _sendPushNotificationEndpointURI;
		private readonly HttpClient _httpClient;

		public FCMAPIHelper(IConfiguration configuration, HttpClient httpClient)
		{
			_httpClient = httpClient;
			var firebaseClientConfiguration = configuration.GetSection("FCMAPI");
			_sendPushNotificationEndpointURI = firebaseClientConfiguration.GetValue<string>("SendPushNotificationEndpointURI");
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

			using (HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_sendPushNotificationEndpointURI, request))
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
	}
}
