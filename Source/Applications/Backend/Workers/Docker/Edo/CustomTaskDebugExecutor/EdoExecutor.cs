using Edo.CodesSaver;
using Edo.Docflow;
using Edo.Documents;
using Edo.Receipt.Dispatcher;
using Edo.Receipt.Sender;
using Edo.Scheduler.Service;
using Edo.Transfer.Dispatcher;
using Edo.Transfer.Sender;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CustomTaskDebugExecutor
{
	public class EdoExecutor
	{
		private readonly IServiceProvider _serviceProvider;

		public EdoExecutor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public async Task TrySendEdoEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("Утилита ручного запуска ЭДО этапов:");
			Console.WriteLine();

			Console.WriteLine("1. EdoRequestCreatedEvent - [Edo.Scheduler]");
			Console.WriteLine("   Выбор типа задачи и создание задачи по заявке");
			Console.WriteLine();

			Console.WriteLine("2. DocumentTaskCreatedEvent - [Edo.Document]");
			Console.WriteLine("   Первичная подготовка данных в задаче на отправку ЭДО документа клиенту");
			Console.WriteLine();

			Console.WriteLine("3. TransferCompleteEvent (для ЭДО документов) - [Edo.Document]");
			Console.WriteLine("   Продолжение обработки после трансфера задачи на отправку ЭДО документа клиенту");
			Console.WriteLine();

			Console.WriteLine("4. OrderDocumentAcceptedEvent - [Edo.Document]");
			Console.WriteLine("   Завершение, после приема клиентом документов, задачи на отправку ЭДО документа клиенту");
			Console.WriteLine();

			Console.WriteLine("5. ReceiptTaskCreatedEvent - [Edo.Receipt.Dispatcher]");
			Console.WriteLine("   Первичная подготовка данных в задаче на отправку чека клиенту");
			Console.WriteLine();

			Console.WriteLine("6. TransferCompleteEvent (для чеков) - [Edo.Receipt.Dispatcher]");
			Console.WriteLine("   Продолжение обработки после трансфера задачи на отправку чека клиенту");
			Console.WriteLine();

			Console.WriteLine("7. ReceiptReadyToSendEvent - [Edo.Receipt.Sender]");
			Console.WriteLine("   Отправка в кассу подготовленных чеков по задаче на отправку чека клиенту");
			Console.WriteLine();

			Console.WriteLine("8. SaveCodesTaskCreatedEvent - [Edo.CodesSaver]");
			Console.WriteLine("   Сохранение кодов приложенных к задаче на сохранение кодов");
			Console.WriteLine();

			Console.WriteLine("9. TransferDocumentSendEvent - [Edo.Docflow]");
			Console.WriteLine("   Подготовка Dto УПД для клиента и отправка ЭДО провайдеру");
			Console.WriteLine();

			Console.WriteLine("10.OrderDocumentSendEvent - [Edo.Docflow]");
			Console.WriteLine("   Подготовка Dto УПД для трансфера и отправка ЭДО провайдеру");
			Console.WriteLine();

			Console.WriteLine("11.TransferRequestCreatedEvent - [Edo.Transfer.Dispatcher]");
			Console.WriteLine("   Распределение заявок на трансфер по направлениям в трансфер задачи");
			Console.WriteLine();

			Console.WriteLine("12.TransferDocumentAcceptedEvent - [Edo.Transfer.Dispatcher]");
			Console.WriteLine("   Перепроверка всех заявок по связанным трансферам после приема задачи на трансфер");
			Console.WriteLine();

			Console.WriteLine("13.TransferTaskReadyToSendEvent - [Edo.Transfer.Sender]");
			Console.WriteLine("   Создание трансфер заказа и трансфер документа");
			Console.WriteLine();

			Console.WriteLine("14.Отправка трансфера (так же как в SendStaleTasks в Edo.Transfer.Routine) - [Edo.Transfer.Dipatcher]");
			Console.WriteLine();

			Console.Write("Выберите действие: ");
			var messageNumber = int.Parse(Console.ReadLine());

			switch(messageNumber)
			{
				case 1:
					await ReceiveEdoRequestCreatedEvent(cancellationToken);
					break;
				case 2:
					await ReceiveDocumentTaskCreatedEvent(cancellationToken);
					break;
				case 3:
					await ReceiveDocumentTransferCompleteEvent(cancellationToken);
					break;
				case 4:
					await ReceiveOrderDocumentAcceptedEvent(cancellationToken);
					break;
				case 5:
					await ReceiveReceiptTaskCreatedEvent(cancellationToken);
					break;
				case 6:
					await ReceiveReceiptTransferCompleteEvent(cancellationToken);
					break;
				case 7:
					await ReceiveReceiptReadyToSendEvent(cancellationToken);
					break;
				case 8:
					await ReceiveSaveCodesTaskCreatedEvent(cancellationToken);
					break;
				case 9:
					await ReceiveTransferDocumentSendEvent(cancellationToken);
					break;
				case 10:
					await ReceiveOrderDocumentSendEvent(cancellationToken);
					break;
				case 11:
					await ReceiveTransferRequestCreatedEvent(cancellationToken);
					break;
				case 12:
					await ReceiveTransferDocumentAcceptedEvent(cancellationToken);
					break;
				case 13:
					await ReceiveTransferTaskReadyToSendEvent(cancellationToken);
					break;
				case 14:
					await SendTransferTask(cancellationToken);
					break;
				default:
					break;
			}
		}

		private async Task ReceiveEdoRequestCreatedEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Необходимо ввести Id клиентской ЭДО заявки (edo_customer_requests)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<EdoTaskScheduler>();
			await service.CreateTask(id, cancellationToken);
		}

		private async Task ReceiveDocumentTaskCreatedEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Необходимо ввести Id задачи с типом Document (edo_tasks)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<DocumentEdoTaskHandler>();
			await service.HandleNew(id, cancellationToken);
		}

		private async Task ReceiveDocumentTransferCompleteEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Завершение трансфера для документов (не чеков)");
			Console.WriteLine("Необходимо ввести Id итерации трансфера (edo_transfer_request_iterations)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<DocumentEdoTaskHandler>();
			await service.HandleTransfered(id, cancellationToken);
		}

		private async Task ReceiveOrderDocumentAcceptedEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Необходимо ввести Id ЭДО документа с типом Order (edo_outgoing_documents)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<DocumentEdoTaskHandler>();
			await service.HandleAccepted(id, cancellationToken);
		}

		private async Task ReceiveReceiptTaskCreatedEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Необходимо ввести Id задачи с типом Receipt (edo_tasks)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<ReceiptEdoTaskHandler>();
			await service.HandleNew(id, cancellationToken);
		}

		private async Task ReceiveReceiptTransferCompleteEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Завершение трансфера для чеков");
			Console.WriteLine("Необходимо ввести Id итерации трансфера (edo_transfer_request_iterations)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<ReceiptEdoTaskHandler>();
			await service.HandleTransfered(id, cancellationToken);
		}

		private async Task ReceiveReceiptReadyToSendEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Необходимо ввести Id задачи с типом Receipt (edo_tasks)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<ReceiptSender>();
			await service.HandleReceiptSendEvent(id, cancellationToken);
		}

		private async Task ReceiveSaveCodesTaskCreatedEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Необходимо ввести Id задачи с типом SaveCodes (edo_tasks)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<SaveCodesEventHandler>();
			await service.Handle(id, cancellationToken);
		}

		private async Task ReceiveTransferDocumentSendEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Необходимо ввести Id ЭДО документа с типом Transfer (edo_outgoing_documents)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<DocflowHandler>();
			await service.HandleTransferDocument(id, cancellationToken);
		}

		private async Task ReceiveOrderDocumentSendEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Необходимо ввести Id ЭДО документа с типом Order (edo_outgoing_documents)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<DocflowHandler>();
			await service.HandleOrderDocument(id, cancellationToken);
		}

		private async Task ReceiveTransferRequestCreatedEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Необходимо ввести Id итерации трансфера (edo_transfer_request_iterations)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<TransferEdoHandler>();
			await service.HandleNewTransfer(id, cancellationToken);
		}

		private async Task ReceiveTransferDocumentAcceptedEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Необходимо ввести Id ЭДО документа с типом Transfer (edo_outgoing_documents)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<TransferEdoHandler>();
			await service.HandleTransferDocumentAcceptance(id, cancellationToken);
		}

		private async Task ReceiveTransferTaskReadyToSendEvent(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Необходимо ввести Id задачи с типом Transfer (edo_tasks)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<TransferSender>();
			await service.HandleReadyToSend(id, cancellationToken);
		}

		private async Task SendTransferTask(CancellationToken cancellationToken)
		{
			Console.WriteLine();
			Console.WriteLine("Необходимо ввести Id задачи с типом Transfer (edo_tasks)");
			Console.Write("Введите Id (0 - выход): ");

			var id = int.Parse(Console.ReadLine());

			if(id <= 0)
			{
				Console.WriteLine("Выход");
				return;
			}

			var service = _serviceProvider.GetRequiredService<TransferEdoHandler>();
			await service.MoveToPrepareToSend(id, cancellationToken);
		}
	}
}
