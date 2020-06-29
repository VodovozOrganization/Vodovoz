using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using Android;

namespace VodovozAndroidDriverService
{
	public class FirebaseCloudMessagingClient : IDriverNotificator
	{
		private readonly string fcmUri;
		private readonly string serverApiKey;
		private readonly string senderId;

		public FirebaseCloudMessagingClient(string serverApiKey, string senderId, string fcmUri = "https://fcm.googleapis.com/fcm/send")
		{
			if(string.IsNullOrWhiteSpace(serverApiKey)) {
				throw new ArgumentNullException(nameof(serverApiKey));
			}

			if(string.IsNullOrWhiteSpace(senderId)) {
				throw new ArgumentNullException(nameof(senderId));
			}

			this.serverApiKey = serverApiKey;
			this.senderId = senderId;
			this.fcmUri = fcmUri;
		}

		public void SendMessage(string deviceId, string sender, string message)
		{
			try {
				var data = new {
					to = deviceId,
					data = new {
						notificationType = "message",
						message = message,
						sender = sender
					}
				};

				SendRequest(data);
			}
			catch(Exception e) {
				Console.WriteLine("{0}\n{1}", e.Message, e.StackTrace);
			}
		}

		public void SendOrderStatusChangeMessage(string deviceId, string sender, string message)
		{
			try {
				var data = new {
					to = deviceId,
					data = new {
						notificationType = "orderStatusChange",
						message = message,
						sender = sender
					}
				};

				SendRequest(data);
			}
			catch(Exception e) {
				Console.WriteLine("{0}\n{1}", e.Message, e.StackTrace);
			}
		}

		public void SendOrderDeliveryScheduleChangeMessage(string deviceId, string sender, string message)
		{
			try {
				var data = new {
					to = deviceId,
					data = new {
						notificationType = "orderDeliveryScheduleChange",
						message = message,
						sender = sender
					}
				};

				SendRequest(data);
			}
			catch(Exception e) {
				Console.WriteLine("{0}\n{1}", e.Message, e.StackTrace);
			}
		}

		public void SendOrderPaymentStatusChangedMessage(string deviceId, string sender, string message)
		{
			try {
				var data = new {
					to = deviceId,
					data = new {
						notificationType = "orderPaymentStatusChange",
						message = message,
						sender = sender
					}
				};

				SendRequest(data);
			}
			catch(Exception e) {
				Console.WriteLine("{0}\n{1}", e.Message, e.StackTrace);
			}
		}

		private string SendRequest(object data)
		{
			try {
				WebRequest request = WebRequest.Create(fcmUri);
				request.Method = "post";
				request.ContentType = "application/json";
				request.Headers.Add(string.Format("Authorization: key={0}", serverApiKey));
				request.Headers.Add(string.Format("Sender: id={0}", senderId));
				var serializer = new JavaScriptSerializer();
				var json = serializer.Serialize(data);
				Byte[] byteArray = Encoding.UTF8.GetBytes(json);
				request.ContentLength = byteArray.Length;
				using(Stream dataStream = request.GetRequestStream()) {
					dataStream.Write(byteArray, 0, byteArray.Length);
					using(WebResponse response = request.GetResponse())
					using(Stream dataStreamResponse = response.GetResponseStream())
					using(StreamReader reader = new StreamReader(dataStreamResponse)) {
						return reader.ReadToEnd();
					}
				}
			}
			catch(Exception e) {
				Console.WriteLine("{0}\n{1}", e.Message, e.StackTrace);
				return String.Empty;
			}
		}
	}
}
