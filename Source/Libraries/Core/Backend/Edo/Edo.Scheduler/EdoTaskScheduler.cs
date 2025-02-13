using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;

namespace Edo.Scheduler.Service
{
	public class EdoTaskScheduler
	{
		private readonly ILogger<EdoTaskScheduler> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<OrderEntity> _orderRepository;
		private readonly OrderTaskScheduler _orderTaskScheduler;
		private readonly BillForAdvanceEdoRequestTaskScheduler _billForAdvanceEdoRequestTaskScheduler;
		private readonly BillForDebtEdoRequestTaskScheduler _billForDebtEdoRequestTaskScheduler;
		private readonly BillForPaymentEdoRequestTaskScheduler _billForPaymentEdoRequestTaskScheduler;
		private readonly IBus _messageBus;

		public EdoTaskScheduler(
			ILogger<EdoTaskScheduler> logger,
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<OrderEntity> orderRepository,
			OrderTaskScheduler orderTaskScheduler,
			BillForAdvanceEdoRequestTaskScheduler billForAdvanceEdoRequestTaskScheduler,
			BillForDebtEdoRequestTaskScheduler billForDebtEdoRequestTaskScheduler,
			BillForPaymentEdoRequestTaskScheduler billForPaymentEdoRequestTaskScheduler,
			IBus messageBus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_orderTaskScheduler = orderTaskScheduler ?? throw new ArgumentNullException(nameof(orderTaskScheduler));
			_billForAdvanceEdoRequestTaskScheduler = billForAdvanceEdoRequestTaskScheduler ?? throw new ArgumentNullException(nameof(billForAdvanceEdoRequestTaskScheduler));
			_billForDebtEdoRequestTaskScheduler = billForDebtEdoRequestTaskScheduler ?? throw new ArgumentNullException(nameof(billForDebtEdoRequestTaskScheduler));
			_billForPaymentEdoRequestTaskScheduler = billForPaymentEdoRequestTaskScheduler ?? throw new ArgumentNullException(nameof(billForPaymentEdoRequestTaskScheduler));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task CreateTask(int requestId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var request = await uow.Session.GetAsync<CustomerEdoRequest>(requestId, cancellationToken);
				if(request == null)
				{
					throw new InvalidOperationException($"В бд нет заявки CustomerEdoRequest id " +
						$"{nameof(CustomerEdoRequest)} {requestId}");
				}

				EdoTask edoTask = null;

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

				await uow.SaveAsync(request, cancellationToken: cancellationToken);
				await uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);

				object message = null;
				switch(edoTask.TaskType)
				{
					case EdoTaskType.Order:
						message = new DocumentTaskCreatedEvent { Id = edoTask.Id };
						break;
					case EdoTaskType.Receipt:
						throw new NotImplementedException("Необходимо реализовать процесс для отправки чека");
						break;
					case EdoTaskType.SaveCode:
						throw new NotImplementedException("Необходимо реализовать процесс для сохранения кодов");
						break;
					case EdoTaskType.BulkAccounting:
						//ничего отправлять пока не нужно, т.к. EdoDocumentsPreparerWorker сам подтянет сущности задач
						break;
					case EdoTaskType.Transfer:
						throw new NotSupportedException("Создание задачи на трансфер из планировщика не предусмотрено");
					default:
						throw new InvalidOperationException($"Неизвестный тип задачи {edoTask.TaskType}");
				}

				await _messageBus.Publish(message, cancellationToken);
			}
		}
	}
}
