using System;
using MassTransit;
using Edo.Contracts.Messages.Events;
using Vodovoz.Core.Domain.Edo;

namespace EdoManualEventSender
{
	public class EdoEventsSender
	{
		private readonly IBus _messageBus;

		public EdoEventsSender(IBus messageBus)
		{
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public void TrySendEdoEvent()
		{
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("Утилита ручной отправки Edo события");
			Console.WriteLine();
			Console.WriteLine("1. EdoRequestCreatedEvent");
			Console.WriteLine("2. DocumentTaskCreatedEvent");
			Console.WriteLine("3. TransferRequestCreatedEvent");
			Console.WriteLine("4. TransferTaskReadyToSendEvent:");
			Console.WriteLine("5. TransferTaskPreparingToSendEvent:");
			Console.WriteLine("6. TransferDocumentSendEvent:");
			Console.WriteLine("7. TransferDocumentAcceptedEvent:");
			Console.WriteLine("8. TransferCompleteEvent (Document):");
			Console.WriteLine("9. TransferCompleteEvent (Receipt):");
			Console.WriteLine("10. OrderDocumentSendEvent:");
			Console.WriteLine("11. OrderDocumentAcceptedEvent:");
			Console.WriteLine("12. SaveCodesTaskCreatedEvent:");
			Console.WriteLine("13. ReceiptTaskCreatedEvent:");
			Console.WriteLine("14. ReceiptReadyToSendEvent:");
			Console.WriteLine();

			Console.Write("Выберите тип сообщения: ");
			var messageNumber = int.Parse(Console.ReadLine());

			switch(messageNumber)
			{
				case 1:
					SendEdoRequestCreatedEvent();
					break;
				case 2:
					SendDocumentTaskCreatedEvent();
					break;
				case 3:
					SendTransferRequestCreatedEvent();
					break;
				case 4:
					SendTransferTaskReadyToSendEvent();
					break;
				case 5:
					SendTransferTaskPreparingToSendEvent();
					break;
				case 6:
					SendTransferDocumentSendEvent();
					break;
				case 7:
					SendTransferDocumentAcceptedEvent();
					break;
				case 8:
					SendTransferCompleteDocumentEvent();
					break;
				case 9:
					SendTransferCompleteReceiptEvent();
					break;
				case 10:
					SendOrderDocumentSendEvent();
					break;
				case 11:
					SendOrderDocumentAcceptedEvent();
					break;
				case 12:
					SendSaveCodesTaskCreatedEvent();
					break;
				case 13:
					SendReceiptTaskCreatedEvent();
					break;
				case 14:
					SendReceiptReadyToSendEvent();
					break;
				default:
					break;
			}
		}

		private void SendEdoRequestCreatedEvent()
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

			_messageBus.Publish(new EdoRequestCreatedEvent { Id = id });
		}

		private void SendDocumentTaskCreatedEvent()
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

			_messageBus.Publish(new DocumentTaskCreatedEvent { Id = id });
		}

		private void SendTransferRequestCreatedEvent()
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

			_messageBus.Publish(new TransferRequestCreatedEvent { TransferIterationId = id });
		}

		private void SendTransferTaskReadyToSendEvent()
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
			_messageBus.Publish(new TransferTaskReadyToSendEvent { TransferTaskId = id });
		}

		private void SendTransferTaskPreparingToSendEvent()
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
			_messageBus.Publish(new TransferTaskPrepareToSendEvent { TransferTaskId = id });
		}
		
		private void SendTransferDocumentSendEvent()
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
			_messageBus.Publish(new TransferDocumentSendEvent { TransferDocumentId = id });
		}

		private void SendTransferDocumentAcceptedEvent()
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
			_messageBus.Publish(new TransferDocumentAcceptedEvent { DocumentId = id });
		}

		private void SendTransferCompleteDocumentEvent()
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
			_messageBus.Publish(new TransferCompleteEvent
			{
				TransferIterationId = id,
				TransferInitiator = TransferInitiator.Document
			});
		}

		private void SendTransferCompleteReceiptEvent()
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
			_messageBus.Publish(new TransferCompleteEvent
			{
				TransferIterationId = id,
				TransferInitiator = TransferInitiator.Receipt
			});
		}

		private void SendOrderDocumentSendEvent()
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
			_messageBus.Publish(new OrderDocumentSendEvent { OrderDocumentId = id });
		}

		private void SendOrderDocumentAcceptedEvent()
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
			_messageBus.Publish(new OrderDocumentAcceptedEvent { DocumentId = id });
		}

		private void SendSaveCodesTaskCreatedEvent()
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
			_messageBus.Publish(new SaveCodesTaskCreatedEvent { EdoTaskId = id });
		}

		private void SendReceiptTaskCreatedEvent()
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
			_messageBus.Publish(new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = id });
		}

		private void SendReceiptReadyToSendEvent()
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
			_messageBus.Publish(new ReceiptReadyToSendEvent { ReceiptEdoTaskId = id });
		}
	}
}
