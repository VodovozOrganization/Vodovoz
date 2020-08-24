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
		
		public static string ServiceAddress = "mango.vod.qsolution.ru";
		//public static string ServiceAddress = "localhost";
		public uint ServicePort = 7087;
		private NotificationService.NotificationServiceClient client;
		private Channel channel;
		private readonly uint extension;
		private readonly CancellationToken token;

		private DateTime? FailSince;

		public bool IsNotificationActive => channel.State == ChannelState.Ready;

		public event EventHandler<ConnectionStateEventArgs> ChanalStateChanged;

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
			var watcher = new NotificationConnectionWatcher(channel, OnChanalStateChanged);

			var responseReaderTask = Task.Run(async () =>
				{
					while(await response.ResponseStream.MoveNext(token))
					{
						FailSince = null;
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
						if (FailSince == null) 
							FailSince = DateTime.Now;
						var failedTime = (DateTime.Now - FailSince).Value;
						if(failedTime.Seconds < 10)
							Thread.Sleep(1000);
						else if(failedTime.Minutes < 10)
							Thread.Sleep(4000);
						else
							Thread.Sleep(30000);
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

		protected virtual void OnChanalStateChanged(ChannelState state)
		{
			ChanalStateChanged?.Invoke(this, new ConnectionStateEventArgs(state));
		}
	}
	
	public class IncomeCallEventArgs : EventArgs
	{
		public NotificationMessage Message { get; set; }
	}

	public class ConnectionStateEventArgs : EventArgs
	{
		public ConnectionStateEventArgs(ChannelState channelState)
		{
			ChannelState = channelState;
		}

		public ChannelState ChannelState { get; }
	}
}
