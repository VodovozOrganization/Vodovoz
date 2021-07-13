using Dapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MangoService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using VodovozMangoService.Calling;

namespace VodovozMangoService.HostedServices
{
	public class NotificationHostedService : NotificationService.NotificationServiceBase, IHostedService
	{
		private readonly MySqlConnection connection;
		private readonly PhonebookHostedService phonebookService;
		private readonly IConfiguration configuration;
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		
		public readonly List<Subscription> Subscribers = new List<Subscription>();

		public NotificationHostedService(MySqlConnection connection, PhonebookHostedService phonebookService, IConfiguration configuration)
		{
			if (phonebookService == null) throw new ArgumentNullException(nameof(phonebookService));
			logger.Info("Создание службы уведомлений");
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
			this.phonebookService = phonebookService;
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		#region GRPC Requests
		public override async Task Subscribe(NotificationSubscribeRequest request, IServerStreamWriter<NotificationMessage> responseStream, ServerCallContext context)
		{
			var subscription = new Subscription(request.Extension);
			lock (Subscribers)
			{
				Subscribers.Add(subscription);
			}
			logger.Debug($"Добавочный {request.Extension} зарегистрировался.");

			try
			{
				var reader = subscription.Queue.Reader;
				while (!context.CancellationToken.IsCancellationRequested)
				{
					var message = await reader.ReadAsync(context.CancellationToken);
					logger.Debug("Сообщение в очереди");
					if (message != null)
						await responseStream.WriteAsync(message);
				}
			}
			catch (Exception e)
			{
				logger.Debug(e);
				throw;
			}
			finally
			{
				lock (Subscribers)
				{
					Subscribers.Remove(subscription);
				}	
				logger.Debug($"Добавочный {request.Extension} отвалился.");
			}
		}
		#endregion

		#region Отправка уведомления
		public void NewEvent(CallInfo info)
		{
			if (!String.IsNullOrEmpty(info.LastEvent.to.extension))
				SendIncome(info);
			
			if (!String.IsNullOrEmpty(info.LastEvent.from.extension))
				SendOutgoing(info);
			
			//В случае если разговор передан на другой внутренний адрес, в разговоре будут уже другие Extension, поэтому события закрытия разговора клиенту не придет....
			//Здесь мы вручную отправляем удедомления тем Extension которые получили событие Connect, но уже не получат Disconnect обычным путем.
			var toDisconnet = info.ConnectedExtensions
					.Where(x => x != info.LastEvent.to.Extension && x != info.LastEvent.from.Extension).ToList();

			if (toDisconnet.Any())
			{
				logger.Debug("toDisconnet:" + String.Join(",", info.ConnectedExtensions));
				IList<Subscription> subscriptions = null;
				lock (Subscribers)
				{
					foreach (var extension in toDisconnet)
					{	
						subscriptions = Subscribers
							.Where(x => x.Extension == extension)
							.ToList();
					}
				}

				if (subscriptions != null && subscriptions.Any())
				{
					var message = new NotificationMessage
					{
						CallId = info.LastEvent.call_id,
						Timestamp = Timestamp.FromDateTimeOffset(info.LastEvent.Time),
						State = CallState.Disconnected,
					};

					SendNotification(subscriptions, message, info);
				}
			}
		}

		private void SendIncome(CallInfo info)
		{
			//Вычисляем получателей
			IList<Subscription> subscriptions;
			lock (Subscribers)
			{
				var count = Subscribers.Count;
				if(count == 0)
					return;

				subscriptions = Subscribers
					.Where(x => x.Extension == info.LastEvent.to.Extension)
					.ToList();
			}
			
			#if DEBUG
			logger.Debug($"Для звонка на {info.LastEvent.to.Extension} подходит {subscriptions.Count} из {Subscribers.Count} подписчиков.");
			#endif
			
			if(subscriptions.Count == 0)
				return; //Не кого уведомлять.
			
			//Подготавливаем сообщение
			var from = info.LastEvent.from;
			Caller caller;
			if (String.IsNullOrEmpty(from.extension))
			{
				if(!String.IsNullOrEmpty(from.number))
					caller = GetExternalCaller(from.number);
				else
				{
					caller = new Caller();
					logger.Error($"Не можем определить кто на линии from.extension и from.number пустые. Событие: {info.LastEvent}");
				}
			}
			else
				caller = GetInternalCaller(from.extension);

			logger.Debug($"Caller:{caller}");
			var message = MakeMessage(info, caller, info.LastEvent.from.extension);
			message.Direction = CallDirection.Incoming;
			SendNotification(subscriptions, message, info);
		}
		
		private void SendOutgoing(CallInfo info)
		{
			//Вычисляем получателей
			IList<Subscription> subscriptions;
			lock (Subscribers)
			{
				var count = Subscribers.Count;
				if(count == 0)
					return;

				subscriptions = Subscribers
					.Where(x => x.Extension == info.LastEvent.from.Extension)
					.ToList();
			}
			
#if DEBUG
			logger.Debug($"Для исходящего с {info.LastEvent.from.Extension} подходит {subscriptions.Count} из {Subscribers.Count} подписчиков.");
#endif
			
			if(subscriptions.Count == 0)
				return; //Не кого уведомлять.
			
			//Подготавливаем сообщение
			var to = info.LastEvent.to;
			Caller caller;
			if (String.IsNullOrEmpty(to.extension))
			{
				if(!String.IsNullOrEmpty(to.number))
					caller = GetExternalCaller(to.number);
				else
				{
					caller = new Caller();
					logger.Error($"Не можем определить кто кому звоним to.extension и to.number пустые. Событие: {info.LastEvent}");
				}
			}
			else
				caller = GetInternalCaller(to.extension);

			logger.Debug($"Caller:{caller}");
			var message = MakeMessage(info, caller, info.LastEvent.from.extension);
			message.Direction = CallDirection.Outgoing;
			SendNotification(subscriptions, message, info);
		}

		private NotificationMessage MakeMessage(CallInfo info, Caller caller, string transferInitiator)
		{ 
			var message = new NotificationMessage
			{
				CallId = info.LastEvent.call_id,
				Timestamp = Timestamp.FromDateTimeOffset(info.LastEvent.Time),
				State = info.LastEvent.CallState,
				CallFrom = caller
			};
			if (info.OnHoldCall != null)
			{
				message.IsTransfer = true;
				if (info.OnHoldCall.LastEvent.from.extension == transferInitiator)
				{
					if (String.IsNullOrEmpty(info.OnHoldCall.LastEvent.to.extension))
					{
						if(!String.IsNullOrWhiteSpace(info.OnHoldCall.LastEvent.to.number))
							message.PrimaryCaller = GetExternalCaller(info.OnHoldCall.LastEvent.to.number);
						else
							logger.Error($"Не можем определить кто на удержании to.extension и to.number пустые. Событие: {info.OnHoldCall.LastEvent}");
					}
					else
						message.PrimaryCaller = GetInternalCaller(info.OnHoldCall.LastEvent.to.extension);
				}
				else
				{
					if (String.IsNullOrEmpty(info.OnHoldCall.LastEvent.from.extension))
					{
						if(!String.IsNullOrWhiteSpace(info.OnHoldCall.LastEvent.from.number))
							message.PrimaryCaller = GetExternalCaller(info.OnHoldCall.LastEvent.from.number);
						else
							logger.Error(
								$"Не можем определить кто на удержании from.extension и from.number пустые. Событие: {info.OnHoldCall.LastEvent}");
					}
					else
						message.PrimaryCaller = GetInternalCaller(info.OnHoldCall.LastEvent.from.extension);	
				}
			}
			return message;
		}
		private void SendNotification(IList<Subscription> subscriptions, NotificationMessage message, CallInfo info)
		{
			#if DEBUG
			logger.Debug($"Отправляем {subscriptions.Count} подписчикам, сообщение: {message}.");
			#endif
			
			// Отправляем уведомление о поступлении входящего
			foreach (var subscription in subscriptions)
			{
				if (subscription.Queue.Reader.CanCount && subscription.Queue.Reader.Count > 5)
				{
					logger.Error($"Подписчик {subscription.Extension} не читает уведомления, видимо сломался, удаляем его.");
					lock (Subscribers)
					{
						Subscribers.Remove(subscription);
					}
					continue;
				}
				subscription.Queue.Writer.WriteAsync(message);
				if(message.State != CallState.Disconnected)
					info.ConnectedExtensions.Add(subscription.Extension);
				else
					info.ConnectedExtensions.Remove(subscription.Extension);
			}
		}
		#endregion
		
		#region External call
		private readonly List<CallerInfoCache> ExternalCallers = new List<CallerInfoCache>();
		private Caller GetExternalCaller(string number)
		{
			CallerInfoCache caller;
			lock (ExternalCallers)
			{
				ExternalCallers.RemoveAll(x => x.LiveTime.TotalMinutes > 5);
				caller = ExternalCallers.Find(x => x.Number == number);
				if (caller != null)
					return caller.Caller;
			}

			logger.Debug($"Поиск...{number}");
			lock (connection)
			{
				caller = ExternalCallers.Find(x => x.Number == number);
				//Здесь проверяем наличие в кеше повтоно, так как если наш поток ожидал разблокировки соединение.
				//Значит другой в этот момент мог положить в кэш полученное значение.
				if (caller != null)  
					return caller.Caller;
				var digits = number.Substring(number.Length - Math.Min(10, number.Length));
				var sql =
					"SELECT counterparty.name as counterparty_name, delivery_points.compiled_address_short as address, CONCAT_WS(\" \", employees.last_name, employees.name, employees.patronymic) as employee_name, " + 
					"phones.employee_id, phones.delivery_point_id, counterparty.id as counterparty_id, subdivisions.short_name as subdivision_name " +
					"FROM phones " +
					"LEFT JOIN employees ON employees.id = phones.employee_id " +
					"LEFT JOIN subdivisions ON subdivisions.id = employees.subdivision_id " +
					"LEFT JOIN delivery_points ON delivery_points.id = phones.delivery_point_id " +
					"LEFT JOIN counterparty ON counterparty.id = phones.counterparty_id OR counterparty.id = delivery_points.counterparty_id " +
					"WHERE phones.digits_number = @digits;";
				var list = connection.Query(sql, new {digits = digits}).ToList();
				logger.Debug($"{list.Count()} телефонов в базе данных.");
				//Очищаем контрагентов у которых номер соответсвует звонящей точке доставки
				list.RemoveAll(x => !String.IsNullOrEmpty(x.counterparty_name) && String.IsNullOrEmpty(x.address) 
					&& list.Any( a => !String.IsNullOrEmpty(a.counterparty_name) && !String.IsNullOrEmpty(a.address)));
				caller = new CallerInfoCache(new Caller
				{
					Number = number,
					Type = CallerType.External,
				});
				foreach (var row in list)
					caller.Caller.Names.Add(new CallerName
						{
							Name = TitleExternalName(row), 
							CounterpartyId = (uint?)row.counterparty_id ?? 0,
							DeliveryPointId = (uint?)row.delivery_point_id ?? 0,
							EmployeeId = (uint?)row.employee_id ?? 0
						});
				lock (ExternalCallers)
				{
					ExternalCallers.Add(caller);
				}
			}
			return caller.Caller;
		}

		private string TitleExternalName(dynamic row)
		{
			if (!string.IsNullOrWhiteSpace(row.employee_name))
				return row.subdivision_name == null ? row.employee_name : $"{row.employee_name} ({row.subdivision_name})";
			if (!string.IsNullOrWhiteSpace(row.address))
				return $"{row.counterparty_name} ({row.address})";
			return row.counterparty_name ?? String.Empty;
		}
		#endregion

		#region Internal Call
		private Caller GetInternalCaller(string number)
		{
			var user = phonebookService.FindPhone(number);
			if(user == null)
				logger.Warn( $"Пришло событие для номера {number}, но его нет в списке пользователей Mango");
			var caller = new Caller
			{
				Type = CallerType.Internal,
				Number = number,
			};
			if (user != null)
			{
				string name = user.Name;
				if (!String.IsNullOrWhiteSpace(user.Department))
					name += $" ({user.Department})";
				caller.Names.Add(new CallerName {Name = name});
			}
			return caller;
		}
		#endregion

		#region IHostedService

		private Server server;
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			logger.Info("Запуск сервера GRPC");
			server = new Server
			{
				Services =
				{
					NotificationService.BindService(this),
					PhonebookService.BindService(phonebookService)
				},
				Ports = { new ServerPort("0.0.0.0", Int32.Parse(configuration["MangoService:grps_client_port"]), ServerCredentials.Insecure) }
			};
			server.Start();
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			logger.Info("Остановка сервера GRPC");
			await server.ShutdownAsync();
		}
		#endregion
	}
}
