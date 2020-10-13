using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MangoService;
using MangoService.DTO.Users;
using MySql.Data.MySqlClient;
using NLog;
using VodovozMangoService.Calling;

namespace VodovozMangoService
{
	public class NotificationHostedService : NotificationService.NotificationServiceBase
	{
		private readonly MySqlConnection connection;
		private readonly MangoController mangoController;
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		
		public readonly List<Subscription> Subscribers = new List<Subscription>();

		public NotificationHostedService(MySqlConnection connection, MangoController mangoController)
		{
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
			this.mangoController = mangoController ?? throw new ArgumentNullException(nameof(mangoController));
		}

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
				while (!context.CancellationToken.IsCancellationRequested)
				{
					var message = subscription.Queue.Take(context.CancellationToken);
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

		public void NewEvent(CallInfo info)
		{
			if (!String.IsNullOrEmpty(info.LastEvent.to.extension))
				SendIncome(info);
			
			if (!String.IsNullOrEmpty(info.LastEvent.from.extension))
				SendOutgoing(info);
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
					.Where(x => x.Extension == info.LastEvent.to.Extension 
					            || x.CurrentCall == info || (x.Extension == 0 && x.CurrentCall == null))
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
				caller = GetExternalCaller(from.number);
			else
				caller = GetInternalCaller(from.extension);

			logger.Debug($"Caller:{caller}");
			var message = MakeMessage(info, caller);
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
				caller = GetExternalCaller(to.number);
			else
				caller = GetInternalCaller(to.extension);

			logger.Debug($"Caller:{caller}");
			var message = MakeMessage(info, caller);
			message.Direction = CallDirection.Outgoing;
			SendNotification(subscriptions, message, info);
		}

		private NotificationMessage MakeMessage(CallInfo info, Caller caller)
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
				if (String.IsNullOrEmpty(info.OnHoldCall.LastEvent.from.extension))
					message.PrimaryCaller = GetExternalCaller(info.OnHoldCall.LastEvent.from.number);
				else
					message.PrimaryCaller = GetInternalCaller(info.OnHoldCall.LastEvent.from.extension);
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
				if (subscription.Queue.Count > 5)
				{
					logger.Warn($"Подписчик {subscription.Extension} не читает уведомления, видимо сломался, удаляем его.");
					lock (Subscribers)
					{
						Subscribers.Remove(subscription);
					}
					continue;
				}
				switch (message.State)
				{
					case CallState.Disconnected: subscription.CurrentCall = null; break; 
					case CallState.Connected: subscription.CurrentCall = info; break;
				}
				subscription.Queue.Add(message);
			}
		}
		
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
		private List<User> Users;
		DateTime lastUpdateUsers = DateTime.MinValue;
		private Caller GetInternalCaller(string number)
		{
			if(Users == null || (DateTime.Now - lastUpdateUsers).TotalMinutes > 5)
				GetUsers();
			var user = Users.Find(x => x.telephony.extension == number);
			if(user == null)
				GetUsers(); //Обновляем на случай если номер добавлен но его нет в кеше.
			user = Users.Find(x => x.telephony.extension == number);
			if(user == null)
				logger.Warn( $"Пришло событие для номера {number}, но его нет в списке пользователей Mango");
			var caller = new Caller
			{
				Type = CallerType.Internal,
				Number = number,
			};
			if (user != null)
			{
				string name = user.general.name;
				if (!String.IsNullOrWhiteSpace(user.general.department))
					name += $" ({user.general.department})";
				caller.Names.Add(new CallerName {Name = name});
			}

			return caller;
		}

		private void GetUsers()
		{
			Users = mangoController.GetAllVPBXEmploies().ToList();
			lastUpdateUsers = DateTime.Now;
		}
		#endregion
	}
}
