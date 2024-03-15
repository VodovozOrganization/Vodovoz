using System;
using System.Text.Json.Serialization;

namespace FirebaseCloudMessaging.Client.Requests
{
	[Serializable]
	public class Notification
	{
		[JsonPropertyName("title")]
		public string Title { get; set; }

		[JsonPropertyName("body")]
		public string Body { get; set; }
	}
}
