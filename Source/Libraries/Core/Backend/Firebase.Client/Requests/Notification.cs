using Newtonsoft.Json;
using System;

namespace Firebase.Client.Requests
{
	[Serializable]
	public class Notification
	{
		[JsonProperty ("title")]
		public string Title { get; set; }

		[JsonProperty("body")]
		public string Body { get; set; }
	}
}
