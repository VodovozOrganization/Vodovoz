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
	/// Сервис обработки проблем с неверным статусом заказа в ЭДО
	/// </summary>
	public class OrderStatusProblemService
	{
		private const string _problemSourceName = "Order.Status";

		private readonly ILogger<OrderStatusProblemService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptionsMonitor<OrderStatusProblemWorkerOptions> _options;
		private readonly IEdoTaskValidator _orderStatusValidator;
		private readonly IEdoTaskValidator _statusValidator;
		private readonly IServiceProvider _serviceProvider;
		private readonly IEdoRepository _edoRepository;
		private readonly IBus _messageBus;

		public OrderStatusProblemService(
			ILogger<OrderStatusProblemService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptionsMonitor<OrderStatusProblemWorkerOptions> options,
			IEnumerable<IEdoTaskValidator> validators,
			IServiceProvider serviceProvider,
			IEdoRepository edoRepository,
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));

			_orderStatusValidator = (validators ?? throw new ArgumentNullException(nameof(validators)))
				.FirstOrDefault(v => v.Name == _problemSourceName)
				?? throw new InvalidOperationException($"Валидатор с именем '{_problemSourceName}' не зарегистрирован");
		}

		private DateTime _minEdoTaskCreationTime => DateTime.Today - _options.CurrentValue.ProblemTimeout;

		/// <summary>
		/// Обработчик задач с неверным статусом заказа в ЭДО
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		public async Task ProcessProblemTasks(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(OrderStatusProblemService)))
			{
				var tasks = await _edoRepository.GetProblemEdoTasks(uow, _problemSourceName, _minEdoTaskCreationTime, cancellationToken);

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
			}

			_logger.LogInformation(
				"Обработка завершена. Всего задач: {Total}. Возобновлено: {Success}. Ошибок: {Errors}",
				edoTasks.Count(),
				successCount,
				errorCount);
		}

		private async Task<bool> TryResumeTask(OrderEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(!_orderStatusValidator.IsApplicable(edoTask))
			{
				_logger.LogError(
					"Задача ЭДО {EdoTaskId} не подходит для обработки проблемы с неверным статусом заказа",
					edoTask.Id);
				return false;
			}

			var validationResult = await _orderStatusValidator.ValidateAsync(edoTask, _serviceProvider, cancellationToken);

			if(!validationResult.IsValid)
			{
				_logger.LogDebug(
					"Задача ЭДО {EdoTaskId}: статус заказа №{OrderId} не подхходит, пропускаем",
					edoTask.Id,
					edoTask.FormalEdoRequest.Order.Id);
				return false;
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId}: статус заказа №{OrderId} подтверждён, возобновляем документооборот",
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

			_logger.LogInformation(
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

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId} (ReceiptEdoTask) находится в статусе {ReceiptStatus}. Публикуем событие {EventName}",
				edoTask.Id,
				edoTask.ReceiptStatus,
				nameof(ReceiptTaskCreatedEvent));

			await _messageBus.Publish(new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = edoTask.Id }, cancellationToken);
		}
	}
}
