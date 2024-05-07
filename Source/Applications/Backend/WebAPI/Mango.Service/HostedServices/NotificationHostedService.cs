using Grpc.Core;
using Mango.Core;
using Mango.Core.Settings;
using Mango.Service.Calling;
using Mango.Service.Extensions;
using Mango.Service.Services;
using MangoService;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Mango;

namespace Mango.Service.HostedServices
{
	public class NotificationHostedService : NotificationService.NotificationServiceBase, IHostedService
	{
		private readonly ILogger<NotificationHostedService> _logger;
		private readonly PhonebookHostedService _phonebookService;
		private readonly IMangoUserSettngs _mangoUserSettngs;
		private readonly IConfiguration _configuration;
		private readonly IMangoConfigurationSettings _mangoSettings;
		private readonly ICallerService _callerService;

		public readonly List<Subscription> Subscribers = new List<Subscription>();

		public NotificationHostedService(
			ILogger<NotificationHostedService> logger,
			PhonebookHostedService phonebookService,
			IMangoUserSettngs mangoUserSettngs,
			IConfiguration configuration,
			IMangoConfigurationSettings mangoSettings,
			ICallerService callerService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_phonebookService = phonebookService ?? throw new ArgumentNullException(nameof(phonebookService));
			_mangoUserSettngs = mangoUserSettngs ?? throw new ArgumentNullException(nameof(mangoUserSettngs));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
			_callerService = callerService ?? throw new ArgumentNullException(nameof(callerService));
			_logger.LogInformation("Создание службы уведомлений");
		}

		#region GRPC Requests
		public override async Task Subscribe(NotificationSubscribeRequest request, IServerStreamWriter<NotificationMessage> responseStream, ServerCallContext context)
		{
			var subscription = new Subscription(request.Extension);
			lock(Subscribers)
			{
				Subscribers.Add(subscription);
			}
			_logger.LogDebug("Добавочный {RequestExtension} зарегистрировался.", request.Extension);

			try
			{
				var reader = subscription.Queue.Reader;
				while(!context.CancellationToken.IsCancellationRequested)
				{
					var message = await reader.ReadAsync(context.CancellationToken);
					_logger.LogDebug("Сообщение в очереди");
					if(message != null)
					{
						await responseStream.WriteAsync(message);
					}
				}
			}
			catch(Exception e)
			{
				_logger.LogDebug(e, "");
				throw;
			}
			finally
			{
				lock(Subscribers)
				{
					Subscribers.Remove(subscription);
				}
				_logger.LogDebug("Добавочный {RequestExtension} отвалился.", request.Extension);
			}
		}
		#endregion

		#region Отправка уведомления
		public void NewEvent(CallInfo info)
		{
			if(info.LastEvent.To.AcdGroup == _mangoUserSettngs.TestCallsGroup)
			{
				return;
			}

			if(!string.IsNullOrEmpty(info.LastEvent.To.Extension))
			{
				SendIncome(info);
			}

			if(!string.IsNullOrEmpty(info.LastEvent.From.Extension))
			{
				SendOutgoing(info);
			}

			//В случае если разговор передан на другой внутренний адрес, в разговоре будут уже другие Extension, поэтому события закрытия разговора клиенту не придет....
			//Здесь мы вручную отправляем удедомления тем Extension которые получили событие Connect, но уже не получат Disconnect обычным путем.
			var toDisconnet = info.ConnectedExtensions
					.Where(x => x != info.LastEvent.To.Extension.ParseExtension() && x != info.LastEvent.From.Extension.ParseExtension()).ToList();

			if(toDisconnet.Any())
			{
				_logger.LogDebug("toDisconnet:" + string.Join(",", info.ConnectedExtensions));
				IList<Subscription> subscriptions = null;
				lock(Subscribers)
				{
					foreach(var extension in toDisconnet)
					{
						subscriptions = Subscribers
							.Where(x => x.Extension == extension)
							.ToList();
					}
				}

				if(subscriptions != null && subscriptions.Any())
				{
					var message = new NotificationMessage
					{
						CallId = info.LastEvent.CallId,
						Timestamp = info.LastEvent.Timestamp.ParseProtoTimestamp(),
						State = CallState.Disconnected,
					};

					SendNotification(subscriptions, message, info);
				}
			}
		}

		private void SendIncome(CallInfo info)
		{
			var extension = info.LastEvent.To.Extension.ParseExtension();

			//Вычисляем получателей
			IList<Subscription> subscriptions;
			lock(Subscribers)
			{
				var count = Subscribers.Count;
				if(count == 0)
				{
					return;
				}

				subscriptions = Subscribers
					.Where(x => x.Extension == extension)
					.ToList();
			}

#if DEBUG
			_logger.LogDebug("Для звонка на {Extension} подходит {SubscriptionsCount} из {SubscribersCount} подписчиков.",
				extension, subscriptions.Count, Subscribers.Count);
#endif

			if(subscriptions.Count == 0)
			{
				return; //Не кого уведомлять.
			}

			//Подготавливаем сообщение
			var from = info.LastEvent.From;
			Caller caller;
			if(string.IsNullOrEmpty(from.Extension))
			{
				if(!string.IsNullOrEmpty(from.Number))
				{
					caller = GetExternalCaller(from.Number);
				}
				else
				{
					caller = new Caller();
					_logger.LogError("Не можем определить кто на линии from.extension и from.number пустые. " +
						"Событие: {LastEvent}", info.LastEvent);
				}
			}
			else
			{
				caller = GetInternalCaller(from.Extension);
			}

			_logger.LogDebug("Caller:{Caller}", caller);
			var message = MakeMessage(info, caller, info.LastEvent.From.Extension);
			message.Direction = CallDirection.Incoming;
			SendNotification(subscriptions, message, info);
		}

		private void SendOutgoing(CallInfo info)
		{
			var extension = info.LastEvent.From.Extension.ParseExtension();
			//Вычисляем получателей
			IList<Subscription> subscriptions;
			lock(Subscribers)
			{
				var count = Subscribers.Count;
				if(count == 0)
				{
					return;
				}

				subscriptions = Subscribers
					.Where(x => x.Extension == extension)
					.ToList();
			}

#if DEBUG
			_logger.LogDebug("Для исходящего с {Extension} подходит {SubscriptionsCount} из {SubscribersCount} подписчиков.",
				extension, subscriptions.Count, Subscribers.Count);
#endif

			if(subscriptions.Count == 0)
			{
				return; //Не кого уведомлять.
			}

			//Подготавливаем сообщение
			var to = info.LastEvent.To;
			Caller caller;
			if(string.IsNullOrEmpty(to.Extension))
			{
				if(!string.IsNullOrEmpty(to.Number))
				{
					caller = GetExternalCaller(to.Number);
				}
				else
				{
					caller = new Caller();
					_logger.LogError("Не можем определить кто кому звоним to.extension и to.number пустые. " +
						"Событие: {LastEvent}", info.LastEvent);
				}
			}
			else
			{
				caller = GetInternalCaller(to.Extension);
			}

			_logger.LogDebug("Caller:{Caller}", caller);
			var message = MakeMessage(info, caller, info.LastEvent.From.Extension);
			message.Direction = CallDirection.Outgoing;
			SendNotification(subscriptions, message, info);
		}

		private NotificationMessage MakeMessage(CallInfo info, Caller caller, string transferInitiator)
		{
			var message = new NotificationMessage
			{
				CallId = info.LastEvent.CallId,
				Timestamp = info.LastEvent.Timestamp.ParseProtoTimestamp(),
				State = (CallState)info.LastEvent.CallState,
				CallFrom = caller
			};
			if(info.OnHoldCall != null)
			{
				message.IsTransfer = true;
				if(info.OnHoldCall.LastEvent.From.Extension == transferInitiator)
				{
					if(string.IsNullOrEmpty(info.OnHoldCall.LastEvent.To.Extension))
					{
						if(!string.IsNullOrWhiteSpace(info.OnHoldCall.LastEvent.To.Number))
						{
							message.PrimaryCaller = GetExternalCaller(info.OnHoldCall.LastEvent.To.Number);
						}
						else
						{
							_logger.LogError("Не можем определить кто на удержании to.extension и to.number пустые. " +
								"Событие: {LastEvent}", info.OnHoldCall.LastEvent);
						}
					}
					else
					{
						message.PrimaryCaller = GetInternalCaller(info.OnHoldCall.LastEvent.To.Extension);
					}
				}
				else
				{
					if(string.IsNullOrEmpty(info.OnHoldCall.LastEvent.From.Extension))
					{
						if(!string.IsNullOrWhiteSpace(info.OnHoldCall.LastEvent.From.Number))
						{
							message.PrimaryCaller = GetExternalCaller(info.OnHoldCall.LastEvent.From.Number);
						}
						else
						{
							_logger.LogError("Не можем определить кто на удержании from.extension и from.number пустые. " +
								"Событие: {LastEvent}", info.OnHoldCall.LastEvent);
						}
					}
					else
					{
						message.PrimaryCaller = GetInternalCaller(info.OnHoldCall.LastEvent.From.Extension);
					}
				}
			}
			return message;
		}

		private void SendNotification(IList<Subscription> subscriptions, NotificationMessage message, CallInfo info)
		{
#if DEBUG
			_logger.LogDebug("Отправляем {SubscriptionsCount} подписчикам, сообщение: {Message}.",
				subscriptions.Count, message);
#endif

			// Отправляем уведомление о поступлении входящего
			foreach(var subscription in subscriptions)
			{
				if(subscription.Queue.Reader.CanCount && subscription.Queue.Reader.Count > 5)
				{
					_logger.LogError("Подписчик {SubscriptionExtension} не читает уведомления, видимо сломался, удаляем его.",
						subscription.Extension);
					lock(Subscribers)
					{
						Subscribers.Remove(subscription);
					}
					continue;
				}
				subscription.Queue.Writer.WriteAsync(message);
				if(message.State != CallState.Disconnected)
				{
					info.ConnectedExtensions.Add(subscription.Extension);
				}
				else
				{
					info.ConnectedExtensions.Remove(subscription.Extension);
				}
			}
		}
		#endregion

		#region External call
		private Caller GetExternalCaller(string number)
		{
			return _callerService.GetExternalCaller(number).Result;
		}

		#endregion

		#region Internal Call
		private Caller GetInternalCaller(string number)
		{
			var user = _phonebookService.FindPhone(number);
			if(user == null)
			{
				_logger.LogWarning("Пришло событие для номера {Number}, но его нет в списке пользователей Mango", number);
			}

			var caller = new Caller
			{
				Type = CallerType.Internal,
				Number = number,
			};
			if(user != null)
			{
				string name = user.Name;
				if(!string.IsNullOrWhiteSpace(user.Department))
				{
					name += $" ({user.Department})";
				}

				caller.Names.Add(new CallerName { Name = name });
			}
			return caller;
		}
		#endregion

		#region IHostedService

		private Server _server;
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			var connectionSettings = _mangoSettings.GrpcConnectionSettings;
			var options = new[]
			{
				new ChannelOption("grpc.keepalive_time_ms", connectionSettings.KeepAliveTimeMs),
				new ChannelOption("grpc.keepalive_timeout_ms", connectionSettings.KeepAliveTimeoutMs),
				new ChannelOption("grpc.keepalive_permit_without_calls", connectionSettings.KeepAlivePermitWithoutCalls ? 1 : 0),
				new ChannelOption("grpc.http2.max_pings_without_data", connectionSettings.MaxPingWithoutData),
				new ChannelOption("grpc.http2.min_time_between_pings_ms", connectionSettings.MinTimeBetweenPingsMs),
				new ChannelOption("grpc.http2.max_ping_strikes", connectionSettings.MaxPingStrikes)
			};

			_logger.LogInformation("Запуск сервера GRPC");
			_server = new Server(options)
			{
				
				Services =
				{
					NotificationService.BindService(this),
					PhonebookService.BindService(_phonebookService)
				},
				Ports = { new ServerPort("0.0.0.0", _mangoSettings.GrpcConnectionSettings.Port, ServerCredentials.Insecure) }
			};
			_server.Start();
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Остановка сервера GRPC");
			await _server.ShutdownAsync();
		}
		#endregion
	}
}
