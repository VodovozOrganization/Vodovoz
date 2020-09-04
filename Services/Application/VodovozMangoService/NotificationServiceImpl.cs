using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NLog;
using VodovozMangoService.Calling;

namespace VodovozMangoService
{
	public class NotificationServiceImpl : NotificationService.NotificationServiceBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		
		public readonly List<Subscription> Subscribers = new List<Subscription>();
		
		public NotificationServiceImpl()
		{
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
			if(String.IsNullOrEmpty(info.LastEvent.to.extension))
				return; //Не знаем кому прислать уведомление.
			
			//Вычисляем получателей
			IList<Subscription> subscriptions;
			lock (Subscribers)
			{
				var count = Subscribers.Count;
				if(count == 0)
					return;

				subscriptions = Subscribers
					.Where(x => x.Extension == info.LastEvent.to.Extension 
					            || (x.Extension == 0 && (x.CurrentCall == null || x.CurrentCall == info)))
					.ToList();
			}
			
			if(subscriptions.Count == 0)
				return; //Не кого уведомлять.
			
			//Подготавливаем сообщение
			var from = info.LastEvent.from;
			Caller caller;
			if (String.IsNullOrEmpty(from.extension))
			{
				caller = new Caller
				{
					Type = CallerType.External,
					Number = from.number
				};
			}
			else
			{
				caller = new Caller
				{
					Type = CallerType.Internal,
					Number = from.extension
				};

			}
			
			var message = new NotificationMessage
			{
			 	CallFrom = caller,
			 	Timestamp = Timestamp.FromDateTimeOffset(info.LastEvent.Time),
			 	State = info.LastEvent.CallState
			};
#if DEBUG
			logger.Debug($"Отправляем {subscriptions.Count} подписчикам, сообщение: {message}.");
#endif
			
			// Отправляем уведомление о поступлении входящего
			foreach (var subscription in subscriptions)
				subscription.Queue.Add(message);
		}
	}
}
