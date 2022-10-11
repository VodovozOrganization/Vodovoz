using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MangoService;
using NLog;

namespace ClientMangoService
{
	public class MangoServiceClient : IDisposable
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		
		public static string ServiceAddress = "mango.vod.qsolution.ru";
		public uint ServicePort = 7087;
		private NotificationService.NotificationServiceClient notificationClient;
		private Channel channel;
		private readonly uint extension;
		private readonly CancellationToken token;

		private DateTime? FailSince;

		public bool IsNotificationActive => channel.State == ChannelState.Ready;

		public event EventHandler<ConnectionStateEventArgs> ChannelStateChanged;

		public MangoServiceClient(uint extension, CancellationToken token)
		{
			this.token = token;
			this.extension = extension;
			Connect();
		}

		#region Notification
		private void Connect()
		{
			channel = new Channel($"{ServiceAddress}:{ServicePort}", ChannelCredentials.Insecure);
			notificationClient = new NotificationService.NotificationServiceClient(channel);

			var request = new NotificationSubscribeRequest { Extension =  extension};
			var response = notificationClient.Subscribe(request);
			var watcher = new NotificationConnectionWatcher(channel, OnChanalStateChanged);

			var responseReaderTask = Task.Run(async () =>
				{
					while(await response.ResponseStream.MoveNext(token))
					{
						FailSince = null;
						var message = response.ResponseStream.Current;
						logger.Debug($"extension:{extension} Received:{message}");
						OnAppearedMessage(message);
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
		public event EventHandler<AppearedMessageEventArgs> AppearedMessage;

		protected virtual void OnAppearedMessage(NotificationMessage message)
		{
			AppearedMessage?.Invoke(this, new AppearedMessageEventArgs{Message = message});
		}
		#endregion

		#region Phonebook

		public List<PhoneEntry> GetPhonebook()
		{
			var client = new PhonebookService.PhonebookServiceClient(channel);
			return client.GetBook(new Empty()).Entries.Where(x => x.Extension != extension).ToList();
		}

		#endregion

		public void Dispose()
		{
			channel.ShutdownAsync();
		}

		protected virtual void OnChanalStateChanged(ChannelState state)
		{
			ChannelStateChanged?.Invoke(this, new ConnectionStateEventArgs(state));
		}
	}
	
	public class AppearedMessageEventArgs : EventArgs
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
