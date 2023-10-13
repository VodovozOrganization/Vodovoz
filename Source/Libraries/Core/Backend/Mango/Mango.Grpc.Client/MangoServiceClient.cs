using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MangoService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Mango;

namespace Mango.Grpc.Client
{
	public class MangoServiceClient : IDisposable
	{
		private readonly ILogger<MangoServiceClient> _logger;
		private readonly IMangoSettings _mangoSettings;
		private readonly uint _extension;
		private readonly CancellationToken _token;
		private NotificationService.NotificationServiceClient _notificationClient;
		private Channel _channel;
		private DateTime? _failSince;

		public bool IsNotificationActive => _channel.State == ChannelState.Ready;

		public event EventHandler<ConnectionStateEventArgs> ChannelStateChanged;

		public MangoServiceClient(ILogger<MangoServiceClient> logger, IMangoSettings mangoSettings, uint extension, CancellationToken token)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
			this._extension = extension;
			this._token = token;
			Connect();
		}

		#region Notification
		private void Connect()
		{
			var url = $"{_mangoSettings.ServiceHost}:{_mangoSettings.ServicePort}";

			var options = new[]
			{
				new ChannelOption("grpc.keepalive_time_ms", _mangoSettings.GrpcKeepAliveTimeMs),
				new ChannelOption("grpc.keepalive_timeout_ms", _mangoSettings.GrpcKeepAliveTimeoutMs),
				new ChannelOption("grpc.keepalive_permit_without_calls", _mangoSettings.GrpcKeepAlivePermitWithoutCalls ? 1 : 0),
				new ChannelOption("grpc.http2.max_pings_without_data", _mangoSettings.GrpcMaxPingWithoutData)
			};

			_channel = new Channel(url, ChannelCredentials.Insecure, options);
			_notificationClient = new NotificationService.NotificationServiceClient(_channel);

			var request = new NotificationSubscribeRequest { Extension =  _extension};
			var response = _notificationClient.Subscribe(request);
			var watcher = new NotificationConnectionWatcher(_channel, OnChanalStateChanged);

			var responseReaderTask = Task.Run(async () =>
				{
					while(await response.ResponseStream.MoveNext(_token))
					{
						_failSince = null;
						var message = response.ResponseStream.Current;
						_logger.LogDebug("Extension:{Extension} Received:{Message}", _extension, message);
						OnAppearedMessage(message);
					}
					_logger.LogWarning("Соединение с NotificationService[{Extension}] завершено.", _extension);
				}, _token).ContinueWith(task =>
				{
					if(task.IsCanceled || (task.Exception?.InnerException as RpcException)?.StatusCode == StatusCode.Cancelled) {
						_logger.LogInformation("Соединение с NotificationService[{Extension}] отменено.", _extension);
					}
					else if (task.IsFaulted)
					{
						if (_failSince == null) 
							_failSince = DateTime.Now;
						var failedTime = (DateTime.Now - _failSince).Value;
						if(failedTime.Seconds < 10)
							Thread.Sleep(1000);
						else if(failedTime.Minutes < 10)
							Thread.Sleep(4000);
						else
							Thread.Sleep(30000);
						_logger.LogError(task.Exception, "");
						_logger.LogInformation("Соединение с NotificationService[{Extension}] разорвано... Пробуем соединиться.", _extension);
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
			var client = new PhonebookService.PhonebookServiceClient(_channel);
			return client.GetBook(new Empty()).Entries.Where(x => x.Extension != _extension).ToList();
		}

		#endregion

		public void Dispose()
		{
			_channel.ShutdownAsync();
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
