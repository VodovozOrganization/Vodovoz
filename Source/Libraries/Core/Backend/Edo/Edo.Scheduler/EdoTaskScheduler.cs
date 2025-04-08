using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Scheduler.Service
{
	public class EdoTaskScheduler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly OrderTaskScheduler _orderTaskScheduler;
		private readonly BillForAdvanceEdoRequestTaskScheduler _billForAdvanceEdoRequestTaskScheduler;
		private readonly BillForDebtEdoRequestTaskScheduler _billForDebtEdoRequestTaskScheduler;
		private readonly BillForPaymentEdoRequestTaskScheduler _billForPaymentEdoRequestTaskScheduler;
		private readonly IBus _messageBus;

		public EdoTaskScheduler(
			IUnitOfWork uow,
			OrderTaskScheduler orderTaskScheduler,
			BillForAdvanceEdoRequestTaskScheduler billForAdvanceEdoRequestTaskScheduler,
			BillForDebtEdoRequestTaskScheduler billForDebtEdoRequestTaskScheduler,
			BillForPaymentEdoRequestTaskScheduler billForPaymentEdoRequestTaskScheduler,
			IBus messageBus
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_orderTaskScheduler = orderTaskScheduler ?? throw new ArgumentNullException(nameof(orderTaskScheduler));
			_billForAdvanceEdoRequestTaskScheduler = billForAdvanceEdoRequestTaskScheduler ?? throw new ArgumentNullException(nameof(billForAdvanceEdoRequestTaskScheduler));
			_billForDebtEdoRequestTaskScheduler = billForDebtEdoRequestTaskScheduler ?? throw new ArgumentNullException(nameof(billForDebtEdoRequestTaskScheduler));
			_billForPaymentEdoRequestTaskScheduler = billForPaymentEdoRequestTaskScheduler ?? throw new ArgumentNullException(nameof(billForPaymentEdoRequestTaskScheduler));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task CreateTask(int requestId, CancellationToken cancellationToken)
		{
			var request = await _uow.Session.GetAsync<CustomerEdoRequest>(requestId, cancellationToken);
			if(request == null)
			{
				throw new InvalidOperationException($"В бд нет заявки CustomerEdoRequest id " +
					$"{nameof(CustomerEdoRequest)} {requestId}");
			}

			EdoTask edoTask = request.Task;
			if(edoTask != null)
			{
				throw new InvalidOperationException($"На заявку id {requestId} уже создана задача id {edoTask.Id}");
			}

			switch(request.Type)
			{
				case CustomerEdoRequestType.Order:
					edoTask = _orderTaskScheduler.CreateTask((OrderEdoRequest)request);
					break;
				case CustomerEdoRequestType.OrderWithoutShipmentForAdvancePayment:
					edoTask = _billForAdvanceEdoRequestTaskScheduler.CreateTask((BillForAdvanceEdoRequest)request);
					break;
				case CustomerEdoRequestType.OrderWithoutShipmentForDebt:
					edoTask = _billForDebtEdoRequestTaskScheduler.CreateTask((BillForDebtEdoRequest)request);
					break;
				case CustomerEdoRequestType.OrderWithoutShipmentForPayment:
					edoTask = _billForPaymentEdoRequestTaskScheduler.CreateTask((BillForPaymentEdoRequest)request);
					break;
				default:
					throw new InvalidOperationException($"Неизвестный тип заявки " +
						$"{nameof(CustomerEdoRequest)} {request.Type}");
			}

			await _uow.SaveAsync(request, cancellationToken: cancellationToken);
			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			object message = null;
			switch(edoTask.TaskType)
			{
				case EdoTaskType.Document:
					message = new DocumentTaskCreatedEvent { Id = edoTask.Id };
					break;
				case EdoTaskType.Withdrawal:
					// создать сообщение для сервиса вывода из оборота
					break;
				case EdoTaskType.Receipt:
					message = new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = edoTask.Id };
					break;
				case EdoTaskType.SaveCode:
					message = new SaveCodesTaskCreatedEvent { EdoTaskId = edoTask.Id };
					break;
				case EdoTaskType.BulkAccounting:
					//ничего отправлять пока не нужно, т.к. EdoDocumentsPreparerWorker сам подтянет сущности задач
					break;
				case EdoTaskType.Transfer:
					throw new NotSupportedException("Создание задачи на трансфер из планировщика не предусмотрено");
				default:
					throw new InvalidOperationException($"Неизвестный тип задачи {edoTask.TaskType}");
			}

			if(message != null)
			{
				await _messageBus.Publish(message, cancellationToken);
			}
			
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
