using System;
using System.Text.Json.Serialization;

namespace FirebaseCloudMessaging.Client.Requests
{
	[Serializable]
	public class SendCloudMessageRequest
	{
		[JsonPropertyName("to")]
		public string To { get; set; }

		[JsonPropertyName("notification")]
		public Notification Notification { get; set; }
	}
}
