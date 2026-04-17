using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	/// <summary>
	/// Сервис обработки проблем с оплатой при самовывозе в ЭДО
	/// </summary>
	public class OrderSelfDeliveryPaidProblemService
	{
		private const string _problemSourceName = "Order.SelfdeliveryPaid";

		private readonly ILogger<OrderSelfDeliveryPaidProblemService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptionsMonitor<OrderSelfDeliveryPaidProblemWorkerOptions> _options;
		private readonly IEdoTaskValidator _selfDeliveryPaidValidator;
		private readonly IServiceProvider _serviceProvider;
		private readonly IBus _messageBus;

		public OrderSelfDeliveryPaidProblemService(
			ILogger<OrderSelfDeliveryPaidProblemService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptionsMonitor<OrderSelfDeliveryPaidProblemWorkerOptions> options,
			IEnumerable<IEdoTaskValidator> validators,
			IServiceProvider serviceProvider,
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options;
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_selfDeliveryPaidValidator = (validators ?? throw new ArgumentNullException(nameof(validators)))
				.FirstOrDefault(v => v.Name == _problemSourceName)
				?? throw new InvalidOperationException($"Валидатор с именем '{_problemSourceName}' не зарегистрирован");
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		private DateTime _minEdoTaskCreationTime => DateTime.Today - _options.CurrentValue.ProblemTimeout;

		/// <summary>
		/// Обработчик задач с проблемой оплаты при самовывозе в ЭДО
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		public async Task ProcessProblemTasks(CancellationToken cancellationToken)
		{
			IList<DocumentEdoTask> tasks;
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				tasks = await GetProblemEdoTasks<DocumentEdoTask>(uow, _problemSourceName, _minEdoTaskCreationTime, cancellationToken);
			}

			_logger.LogInformation("Найдено {Count} задач ЭДО с активной проблемой {ProblemName}",
				tasks.Count, _problemSourceName);

			var successCount = 0;
			var errorCount = 0;

			foreach(var edoTask in tasks)
			{
				try
				{
					var resumed = await TryResumeTask(edoTask, cancellationToken);
					if(resumed)
					{
						successCount++;
					}
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка при обработке задачи ЭДО {EdoTaskId}", edoTask.Id);
					errorCount++;
				}
			}

			_logger.LogInformation(
				"Обработка завершена. Всего задач: {Total}. Возобновлено: {Success}. Ошибок: {Errors}",
				tasks.Count,
				successCount,
				errorCount);
		}

		private async Task<IList<T>> GetProblemEdoTasks<T>(
			IUnitOfWork uow,
			string problemSourceName,
			DateTime minCreationTime,
			CancellationToken cancellationToken)
			where T : OrderEdoTask
		{
			var tasksIdsQuery =
				from problem in uow.Session.Query<EdoTaskProblem>()
				join edoTask in uow.Session.Query<T>() on problem.EdoTask.Id equals edoTask.Id
				join edoRequest in uow.Session.Query<FormalEdoRequest>() on edoTask.FormalEdoRequest.Id equals edoRequest.Id
				where
					problem.SourceName == problemSourceName
					&& problem.State == TaskProblemState.Active
					&& edoTask.CreationTime >= minCreationTime
				select edoTask.Id;

			var taskIds = await tasksIdsQuery.Distinct().ToListAsync(cancellationToken);

			if(!taskIds.Any())
			{
				return new List<T>();
			}

			var tasks = await uow.Session.Query<T>()
				.Where(t => taskIds.Contains(t.Id))
				.Fetch(t => t.FormalEdoRequest)
				.ThenFetch(r => r.Order)
				.ToListAsync(cancellationToken);

			return tasks;
		}

		private async Task<bool> TryResumeTask(OrderEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(_selfDeliveryPaidValidator.IsApplicable(edoTask))
			{
				_logger.LogError(
					"Задача ЭДО {EedoTaskId} не подходит для обработки проблемой оплаты при самовывозе",
					edoTask.Id);
				return false;
			}

			var validationResult = await _selfDeliveryPaidValidator.ValidateAsync(edoTask, _serviceProvider, cancellationToken);

			if(!validationResult.IsValid)
			{
				_logger.LogDebug(
					"Задача ЭДО {EdoTaskId}: оплата самовывоза по заказу №{OrderId} ещё не подтверждена, пропускаем",
					edoTask.Id,
					edoTask.FormalEdoRequest.Order.Id);
				return false;
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId}: оплата самовывоза по заказу №{OrderId} подтверждена, возобновляем документооборот",
				edoTask.Id,
				edoTask.FormalEdoRequest.Order.Id);

			await PublishResumeEvent(edoTask, cancellationToken);

			return true;
		}

		private async Task PublishResumeEvent(OrderEdoTask edoTask, CancellationToken cancellationToken)
		{
			switch(edoTask)
			{
				case DocumentEdoTask documentTask:
					await PublishDocumentResumeEvent(documentTask, cancellationToken);
					break;
				case TenderEdoTask tenderTask:
					await PublishTenderResumeEvent(tenderTask, cancellationToken);
					break;
				case ReceiptEdoTask receiptTask:
					await PublishReceiptResumeEvent(receiptTask, cancellationToken);
					break;
				default:
					_logger.LogWarning(
						"Задача ЭДО {EdoTaskId}: неизвестный тип задачи {TaskType}, не удалось определить событие для возобновления",
						edoTask.Id, edoTask.GetType().Name);
					break;
			}
		}

		private async Task PublishDocumentResumeEvent(DocumentEdoTask edoTask, CancellationToken cancellationToken)
		{
			switch(edoTask.Stage)
			{
				case DocumentEdoTaskStage.New:
					{
						_logger.LogInformation(
							"Задача ЭДО {EdoTaskId} (DocumentEdoTask, Stage=New): публикуем DocumentTaskCreatedEvent",
							edoTask.Id);
						await _messageBus.Publish(new DocumentTaskCreatedEvent { Id = edoTask.Id }, cancellationToken);
						break;
					}
				case DocumentEdoTaskStage.Sending:
					{
						_logger.LogInformation(
							"Задача ЭДО {EdoTaskId} (DocumentEdoTask, Stage=Sending): публикуем OrderDocumentSentEvent",
							edoTask.Id);
						await _messageBus.Publish(new OrderDocumentSentEvent { Id = edoTask.Id }, cancellationToken);
						break;
					}
				case DocumentEdoTaskStage.Sent:
					{
						var documentId = await GetOrderEdoDocumentId(edoTask.Id, cancellationToken);
						if(documentId == null)
						{
							_logger.LogWarning(
								"Задача ЭДО {EdoTaskId} (DocumentEdoTask, Stage=Sent): не найден связанный документ, пропускаем",
								edoTask.Id);
							return;
						}
						_logger.LogInformation(
							"Задача ЭДО {EdoTaskId} (DocumentEdoTask, Stage=Sent): публикуем OrderDocumentAcceptedEvent (DocumentId={DocumentId})",
							edoTask.Id,
							documentId.Value);

						await _messageBus.Publish(new OrderDocumentAcceptedEvent { DocumentId = documentId.Value }, cancellationToken);
						break;
					}
				default:
					{
						_logger.LogWarning(
							"Задача ЭДО {EdoTaskId} (DocumentEdoTask): не удалось определить событие для этапа {Stage}",
							edoTask.Id, edoTask.Stage);
						break;
					}
			}
		}

		private async Task PublishTenderResumeEvent(TenderEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.Stage != TenderEdoTaskStage.New)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} (TenderEdoTask): не удалось определить событие для этапа {Stage}",
					edoTask.Id,
					edoTask.Stage);
				return;
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId} (TenderEdoTask, Stage=New): публикуем TenderTaskCreatedEvent",
				edoTask.Id);

			await _messageBus.Publish(new TenderTaskCreatedEvent { TenderEdoTaskId = edoTask.Id }, cancellationToken);
		}

		private async Task PublishReceiptResumeEvent(ReceiptEdoTask edoTask, CancellationToken cancellationToken)
		{
			switch(edoTask.ReceiptStatus)
			{
				case EdoReceiptStatus.New:
					{
						_logger.LogInformation(
							"Задача ЭДО {EdoTaskId} (ReceiptEdoTask, ReceiptStatus=New): публикуем ReceiptTaskCreatedEvent",
							edoTask.Id);
						await _messageBus.Publish(new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = edoTask.Id }, cancellationToken);
						break;
					}
				case EdoReceiptStatus.Transfering:
					{
						var iterationId = await GetCompletedReceiptTransferIterationId(edoTask.Id, cancellationToken);
						if(iterationId == null)
						{
							_logger.LogWarning(
								"Задача ЭДО {EdoTaskId} (ReceiptEdoTask, ReceiptStatus=Transfering): не найдена завершённая итерация трансфера, пропускаем",
								edoTask.Id);
							return;
						}
						_logger.LogInformation(
							"Задача ЭДО {EdoTaskId} (ReceiptEdoTask, ReceiptStatus=Transfering): публикуем TransferCompleteEvent (IterationId={IterationId})",
							edoTask.Id, iterationId.Value);
						await _messageBus.Publish(
							new TransferCompleteEvent { TransferIterationId = iterationId.Value, TransferInitiator = TransferInitiator.Receipt },
							cancellationToken);
						break;
					}
				case EdoReceiptStatus.Sending:
					{
						_logger.LogInformation(
							"Задача ЭДО {EdoTaskId} (ReceiptEdoTask, ReceiptStatus=Sending): публикуем ReceiptReadyToSendEvent",
							edoTask.Id);
						await _messageBus.Publish(new ReceiptReadyToSendEvent { ReceiptEdoTaskId = edoTask.Id }, cancellationToken);
						break;
					}
				default:
					{
						_logger.LogWarning(
							"Задача ЭДО {EdoTaskId} (ReceiptEdoTask): не удалось определить событие для ReceiptStatus={ReceiptStatus}",
							edoTask.Id, edoTask.ReceiptStatus);
						break;
					}
			}
		}

		private async Task<int?> GetOrderEdoDocumentId(int documentTaskId, CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				return await uow.Session.Query<OrderEdoDocument>()
					.Where(d => d.DocumentTaskId == documentTaskId)
					.Select(d => (int?)d.Id)
					.FirstOrDefaultAsync(cancellationToken);
			}
		}

		private async Task<int?> GetCompletedReceiptTransferIterationId(int receiptTaskId, CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				return await uow.Session.Query<TransferEdoRequestIteration>()
					.Where(i => i.OrderEdoTask.Id == receiptTaskId
								&& i.Initiator == TransferInitiator.Receipt
								&& i.Status == TransferEdoRequestIterationStatus.Completed)
					.OrderByDescending(i => i.Id)
					.Select(i => (int?)i.Id)
					.FirstOrDefaultAsync(cancellationToken);
			}
		}
	}
}
