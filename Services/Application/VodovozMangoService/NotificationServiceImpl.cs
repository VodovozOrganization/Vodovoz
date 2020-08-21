using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;

namespace VodovozMangoService
{
	public class NotificationServiceImpl : NotificationService.NotificationServiceBase
	{
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

			while (!context.CancellationToken.IsCancellationRequested)
			{
				var message = queue.Take(context.CancellationToken);
				if(message != null)
					await responseStream.WriteAsync(message);
			}

			lock (Subscribers)
			{
				if (Subscribers.ContainsValue(queue))
					Subscribers.Remove(request.Extension);
			}
		}
	}
}
