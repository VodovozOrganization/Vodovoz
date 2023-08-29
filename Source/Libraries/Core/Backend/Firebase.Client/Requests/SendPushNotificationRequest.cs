using Newtonsoft.Json;
using System;

namespace Firebase.Client.Requests
{
	[Serializable]
	public class SendPushNotificationRequest
	{
		[JsonProperty("to")]
		public string To { get; set; }

		[JsonProperty("notification")]
		public Notification Notification { get; set; }
	}
}
