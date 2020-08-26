using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
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
				//Ждем
				var wait = random.Next(1000, 7000);
				Console.WriteLine($"Ждем {wait} милисекуд");
				Thread.Sleep(wait);
				//Выбираем подписчика
				BlockingCollection<NotificationMessage> queue;
				lock (service.Subscribers)
				{
					var count = service.Subscribers.Count;
					if(count == 0)
						continue;
					
					queue = service.Subscribers.Values.Skip(random.Next(count)).First();
				}
				//Создаем звонящего....
				var type = (CallerType) random.Next(2);
				var caller = new Caller
				{
					Type = type,
					Number = type == CallerType.External
						? "+79" + random.Next(1000000000).ToString("D9")
						: random.Next(100, 1000).ToString(),
					Name = type == CallerType.Internal ? new [] {"Ганьков Андрей", "Солдаткина Алина", "Потапов Данила"}[random.Next(3)] : String.Empty
				};
				//Отправляем уведомление о поступлении входящего
				var message = new NotificationMessage
				{
					CallFrom = caller,
					Timestamp = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
					State = CallState.Appeared
				};
				Console.WriteLine("Send:" + message.ToString() );
				queue.Add(message);
				//Ждет поднятия трубки
				wait = random.Next(1000, 10000);
				Console.WriteLine($"Ждем {wait} милисекуд, пока оператор поднимет трубку.");
				Thread.Sleep(wait);
				//Отправляем уведомление о начале разговора
				message = new NotificationMessage
				{
					CallFrom = caller,
					Timestamp = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
					State = CallState.Connected
				};
				Console.WriteLine("Send:" + message.ToString() );
				queue.Add(message);
				//Ожидаем разговор.
				wait = random.Next(5000, 25000);
				Console.WriteLine($"Идет разговор с оператором {wait} милисекуд.");
				Thread.Sleep(wait);
				
				//Отправляем уведомление о завершении разговора.
				message = new NotificationMessage
				{
					CallFrom = caller,
					Timestamp = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
					State = CallState.Disconnected
				};
				Console.WriteLine("Send:" + message.ToString() );
				queue.Add(message);
			}
		}
	}
}
