using Microsoft.Extensions.DependencyInjection;
using System;
using Edo.Transport;
using Vodovoz.Settings.Pacs;
using System.Collections.Generic;
using MassTransit;
using Edo.Contracts.Messages.Events;

namespace CustomerEdoRequestSenderTEST
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			ServiceCollection services = new ServiceCollection();
			services.AddScoped<IMessageTransportSettings, TESTMessageTransportSettings>();
			services.AddEdoMassTransit();

			var serviceProvider = services.BuildServiceProvider();
			var messageBus = serviceProvider.GetRequiredService<IBus>();

			while(true)
			{
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine($"1. EdoRequestCreatedEvent:");
				Console.WriteLine($"2. DocumentTaskCreatedEvent:");
				Console.WriteLine($"3. TransferRequestCreatedEvent:");
				Console.WriteLine($"4. TransferTaskReadyToSendEvent:");
				Console.WriteLine($"5. TransferDocumentSendEvent:");
				Console.WriteLine($"6. TransferDocumentAcceptedEvent:");
				Console.WriteLine($"7. TransferDoneEvent:");

				Console.Write($"Select message: ");
				var messageType = int.Parse(Console.ReadLine());
				var messageTypeName = string.Empty;

				Console.Write($"Enter message id: ");
				var id = int.Parse(Console.ReadLine());

				object message = null;
				switch(messageType)
				{
					case 1:
						messageTypeName = "EdoRequestCreatedEvent";
						message = new EdoRequestCreatedEvent { Id = id };
						break;
					case 2:
						messageTypeName = "DocumentTaskCreatedEvent";
						message = new DocumentTaskCreatedEvent { Id = id };
						break;
					case 3:
						messageTypeName = "TransferRequestCreatedEvent";
						message = new TransferRequestCreatedEvent { DocumentEdoTaskId = id };
						break;
					case 4:
						messageTypeName = "TransferTaskReadyToSendEvent";
						message = new TransferTaskReadyToSendEvent { Id = id };
						break;
					case 5:
						messageTypeName = "TransferDocumentSendEvent";
						message = new TransferDocumentSendEvent { Id = id };
						break;
					case 6:
						messageTypeName = "TransferDocumentAcceptedEvent";
						message = new TransferDocumentAcceptedEvent { DocumentId = id };
						break;
					case 7:
						messageTypeName = "TransferDoneEvent";
						message = new TransferDoneEvent { Id = id };
						break;

						
				}
				
				messageBus.Publish(message);
				Console.WriteLine($"Published {messageTypeName} with id {id}");
			}
		}
	}

	public class TESTMessageTransportSettings : IMessageTransportSettings
	{
		public string Host => "rabbit.vod.qsolution.ru";
		public int Port => 5671;
		public string VirtualHost => "edo_test";
		public string Username => "edo_test";
		public string Password => "Hs62rb>cvn92;brt@#slu";
		public bool UseSSL => true;
		public List<MessageTTLSetting> MessagesTimeToLive => new List<MessageTTLSetting>();
		public string AllowSslPolicyErrors => "";
		public bool TestMode => false;
	}
}
