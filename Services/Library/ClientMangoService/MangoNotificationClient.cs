using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace ClientMangoService
{
	public class MangoNotificationClient
	{
		// public static string ServiceAddress = "mango.vod.qsolution.ru";
		public static string ServiceAddress = "localhost";
		public uint ServicePort = 7087;
		private NotificationService.NotificationServiceClient client;

		public MangoNotificationClient(uint extension, CancellationToken token)
		{
			Channel channel = new Channel($"{ServiceAddress}:{ServicePort}", ChannelCredentials.Insecure);
			client = new NotificationService.NotificationServiceClient(channel);
			
			var request = new NotificationSubscribeRequest { Extension =  extension};
			var response = client.Subscribe(request);


			var responseReaderTask = Task.Run(async () =>
			{
				while(await response.ResponseStream.MoveNext()) {
					var message = response.ResponseStream.Current;
					Console.WriteLine($"extension:{extension} Received:{message.Number}");
				}
			});
		}

		public event EventHandler<IncomeCallEventArgs> IncomeCall;

		protected virtual void OnIncomeCall(NotificationMessage message)
		{
			IncomeCall?.Invoke(this, new IncomeCallEventArgs{Message = message});
		}
	}
	
	public class IncomeCallEventArgs : EventArgs
	{
		public NotificationMessage Message { get; set; }
	}
}
