using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Grpc.Core;

namespace VodovozMangoService
{
	class Program
	{
		static void Main(string[] args)
		{
			const int Port = 7087;
			var service = new NotificationServiceImpl();
			Server server = new Server
			{
				Services = { NotificationService.BindService(service) },
				Ports = { new ServerPort("0.0.0.0", Port, ServerCredentials.Insecure) }
			};
			server.Start();
			
			RandomNotifyTest(service);
			
			Console.WriteLine("NotificationService server listening on port " + Port);
			Console.WriteLine("Press any key to stop the server...");
			Console.ReadKey();

			server.ShutdownAsync().Wait();
		}

		private static void RandomNotifyTest(NotificationServiceImpl service)
		{
			var random = new Random();
			while (true)
			{
				var wait = random.Next(20, 5000);
				Console.WriteLine($"Ждем {wait} милисекуд");
				Thread.Sleep(wait);

				BlockingCollection<NotificationMessage> queue;
				lock (service.Subscribers)
				{
					var count = service.Subscribers.Count;
					if(count == 0)
						continue;
					
					queue = service.Subscribers.Values.Skip(random.Next(count)).First();
				}

				var message = new NotificationMessage
				{
					Number = "+79" + random.Next(1000000000).ToString("D9")
				};
				Console.WriteLine("Send:" + message.Number );
				queue.Add(message);
			}
		}
	}
}
