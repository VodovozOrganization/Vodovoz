using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
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
		private readonly IEdoRepository _edoRepository;
		private readonly IBus _messageBus;

		public OrderSelfDeliveryPaidProblemService(
			ILogger<OrderSelfDeliveryPaidProblemService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptionsMonitor<OrderSelfDeliveryPaidProblemWorkerOptions> options,
			IEnumerable<IEdoTaskValidator> validators,
			IServiceProvider serviceProvider,
			IEdoRepository edoRepository,
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options;
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
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
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(OrderSelfDeliveryPaidProblemService)))
			{
				var documentTasks =
					await _edoRepository.GetProblemEdoTasks<DocumentEdoTask>(uow, _problemSourceName, _minEdoTaskCreationTime, cancellationToken);
				var receiptTasks =
					await _edoRepository.GetProblemEdoTasks<ReceiptEdoTask>(uow, _problemSourceName, _minEdoTaskCreationTime, cancellationToken);
				var tenderTasks =
					await _edoRepository.GetProblemEdoTasks<TenderEdoTask>(uow, _problemSourceName, _minEdoTaskCreationTime, cancellationToken);

				var tasks = documentTasks
					.Concat<OrderEdoTask>(receiptTasks)
					.Concat(tenderTasks)
					.ToList();

				_logger.LogInformation("Найдено {Count} задач ЭДО с активной проблемой {ProblemName}",
					tasks.Count, _problemSourceName);

				await TryResumeTasks(tasks, cancellationToken);
			}
		}

		private async Task TryResumeTasks(IEnumerable<OrderEdoTask> edoTasks, CancellationToken cancellationToken)
		{
			var successCount = 0;
			var errorCount = 0;

			foreach(var edoTask in edoTasks)
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

				_logger.LogInformation(
					"Обработка завершена. Всего задач: {Total}. Возобновлено: {Success}. Ошибок: {Errors}",
					edoTasks.Count(),
					successCount,
					errorCount);
			}
		}

		private async Task<bool> TryResumeTask(OrderEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(!_selfDeliveryPaidValidator.IsApplicable(edoTask))
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
			if(edoTask.Stage != DocumentEdoTaskStage.New)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} (DocumentEdoTask) находится на стадии {Stage}. Возобновление возможно только на стадии New",
					edoTask.Id,
					edoTask.Stage);
				return;
			}

			_logger.LogWarning(
				"Задача ЭДО {EdoTaskId} (DocumentEdoTask) находится на стадии {Stage}. Публикуем событие {EventName}",
				edoTask.Id,
				edoTask.Stage,
				nameof(DocumentTaskCreatedEvent));

			await _messageBus.Publish(new DocumentTaskCreatedEvent { Id = edoTask.Id }, cancellationToken);
		}

		private async Task PublishTenderResumeEvent(TenderEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.Stage != TenderEdoTaskStage.New)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} (TenderEdoTask) находится на стадии {Stage}. Возобновление возможно только на стадии New",
					edoTask.Id,
					edoTask.Stage);
				return;
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId} (TenderEdoTask) находится на стадии {Stage}. Публикуем событие {EventName}",
				edoTask.Id,
				edoTask.Stage,
				nameof(TenderTaskCreatedEvent));

			await _messageBus.Publish(new TenderTaskCreatedEvent { TenderEdoTaskId = edoTask.Id }, cancellationToken);
		}

		private async Task PublishReceiptResumeEvent(ReceiptEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.ReceiptStatus != EdoReceiptStatus.New)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} (ReceiptEdoTask) находится в статусе {ReceiptStatus}. Возобновление возможно только в статусе New",
					edoTask.Id,
					edoTask.ReceiptStatus);
				return;
			}

			_logger.LogWarning(
				"Задача ЭДО {EdoTaskId} (ReceiptEdoTask) находится в статусе {ReceiptStatus}. Публикуем событие {EventName}",
				edoTask.Id,
				edoTask.ReceiptStatus,
				nameof(ReceiptTaskCreatedEvent));

			await _messageBus.Publish(new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = edoTask.Id }, cancellationToken);
		}
	}
}
