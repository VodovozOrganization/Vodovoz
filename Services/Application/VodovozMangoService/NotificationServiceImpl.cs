using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using NLog;

namespace VodovozMangoService
{
	public class NotificationServiceImpl : NotificationService.NotificationServiceBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		
		public readonly Dictionary<uint, BlockingCollection<NotificationMessage>> Subscribers =
			new Dictionary<uint, BlockingCollection<NotificationMessage>>();
		public NotificationServiceImpl()
		{
		}

		public override async Task Subscribe(NotificationSubscribeRequest request, IServerStreamWriter<NotificationMessage> responseStream, ServerCallContext context)
		{
			var queue = new BlockingCollection<NotificationMessage>();
			lock (Subscribers)
			{
				Subscribers[request.Extension] = queue;
			}
			logger.Debug($"Добавочный {request.Extension} зарегистрировался.");

			try
			{
				while (!context.CancellationToken.IsCancellationRequested)
				{
					var message = queue.Take(context.CancellationToken);
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
					if (Subscribers.ContainsValue(queue))
					{
						Subscribers.Remove(request.Extension);
						logger.Debug($"Добавочный {request.Extension} отвалился.");
					}
				}	
			}
		}
	}
}
