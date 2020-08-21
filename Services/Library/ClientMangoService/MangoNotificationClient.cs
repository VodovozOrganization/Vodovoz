using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NLog;

namespace ClientMangoService
{
	public class MangoNotificationClient : IDisposable
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		
		// public static string ServiceAddress = "mango.vod.qsolution.ru";
		public static string ServiceAddress = "localhost";
		public uint ServicePort = 7087;
		private NotificationService.NotificationServiceClient client;
		private Channel channel;
		private readonly uint extension;
		private readonly CancellationToken token;

		public MangoNotificationClient(uint extension, CancellationToken token)
		{
			this.token = token;
			this.extension = extension;
			Connect();
		}

		private void Connect()
		{
			channel = new Channel($"{ServiceAddress}:{ServicePort}", ChannelCredentials.Insecure);
			client = new NotificationService.NotificationServiceClient(channel);

			var request = new NotificationSubscribeRequest { Extension =  extension};
			var response = client.Subscribe(request);
			Console.WriteLine($"Channel State: {channel.State}");

			var responseReaderTask = Task.Run(async () =>
				{
					while(await response.ResponseStream.MoveNext(token)) {
						var message = response.ResponseStream.Current;
						Console.WriteLine($"extension:{extension} Received:{message.Number}");
						OnIncomeCall(message);
					}
					logger.Warn($"Соединение с NotificationService[{extension}] завершено.");
				}, token).ContinueWith(task =>
				{
					if(task.IsCanceled || (task.Exception?.InnerException as RpcException)?.StatusCode == StatusCode.Cancelled) {
						logger.Info($"Соединение с NotificationService[{extension}] отменено.");
					}
					else if (task.IsFaulted)
					{
						logger.Error(task.Exception);
						logger.Info($"Соединение с NotificationService[{extension}] разорвано... Пробуем соединиться.");
						Connect();
					} 
				})
				;
		}
		public event EventHandler<IncomeCallEventArgs> IncomeCall;

		protected virtual void OnIncomeCall(NotificationMessage message)
		{
			IncomeCall?.Invoke(this, new IncomeCallEventArgs{Message = message});
		}

		public void Dispose()
		{
			channel.ShutdownAsync();
		}
	}
	
	public class IncomeCallEventArgs : EventArgs
	{
		public NotificationMessage Message { get; set; }
	}
}
