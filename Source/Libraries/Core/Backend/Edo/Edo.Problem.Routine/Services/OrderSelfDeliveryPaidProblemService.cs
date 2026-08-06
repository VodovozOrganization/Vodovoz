using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using Edo.Problems.Validation;
using Edo.Transport;
using EdoNotifications.Contracts;
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
		private readonly IEdoProblemRoutineNotificationService _notificationService;
		private readonly MessageService _messageService;

		public OrderSelfDeliveryPaidProblemService(
			ILogger<OrderSelfDeliveryPaidProblemService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptionsMonitor<OrderSelfDeliveryPaidProblemWorkerOptions> options,
			IEnumerable<IEdoTaskValidator> validators,
			IServiceProvider serviceProvider,
			IEdoRepository edoRepository,
			IBus messageBus,
			IEdoProblemRoutineNotificationService notificationService,
			MessageService messageService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_selfDeliveryPaidValidator = (validators ?? throw new ArgumentNullException(nameof(validators)))
				.FirstOrDefault(v => v.Name == _problemSourceName)
				?? throw new InvalidOperationException($"Валидатор с именем '{_problemSourceName}' не зарегистрирован");
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
			_messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
		}

		private DateTime _minEdoTaskCreationTime => DateTime.Today - _options.CurrentValue.ProblemTimeout;

		/// <summary>
		/// Обработчик задач с проблемой оплаты при самовывозе в ЭДО
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		public async Task ProcessProblemTasks(CancellationToken cancellationToken)
		{
			IList<int> taskIds;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(OrderSelfDeliveryPaidProblemService)))
			{
				var documentTasks =
					await _edoRepository.GetProblemEdoTasks<DocumentEdoTask>(uow, _problemSourceName, _minEdoTaskCreationTime, cancellationToken);
				var receiptTasks =
					await _edoRepository.GetProblemEdoTasks<ReceiptEdoTask>(uow, _problemSourceName, _minEdoTaskCreationTime, cancellationToken);
				var tenderTasks =
					await _edoRepository.GetProblemEdoTasks<TenderEdoTask>(uow, _problemSourceName, _minEdoTaskCreationTime, cancellationToken);

				taskIds = documentTasks
					.Select(x => x.Id)
					.Concat(receiptTasks.Select(x => x.Id))
					.Concat(tenderTasks.Select(x => x.Id))
					.ToList();

				_logger.LogInformation("Найдено {Count} задач ЭДО с активной проблемой {ProblemName}",
					taskIds.Count, _problemSourceName);
			}

			await TryResumeTasks(taskIds, cancellationToken);
		}

		private async Task TryResumeTasks(IList<int> taskIds, CancellationToken cancellationToken)
		{
			var successCount = 0;
			var errorCount = 0;

			foreach(var taskId in taskIds)
			{
				try
				{
					using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(OrderSelfDeliveryPaidProblemService)))
					{
						var edoTask = await uow.Session.GetAsync<OrderEdoTask>(taskId, cancellationToken);

						if(edoTask == null)
						{
							_logger.LogWarning("Задача ЭДО {EdoTaskId} не найдена", taskId);
							continue;
						}

						var resumed = await TryResumeTask(uow, edoTask, cancellationToken);
						if(resumed)
						{
							successCount++;
						}
					}
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка при обработке задачи ЭДО {EdoTaskId}", taskId);
					errorCount++;
				}
			}

			_logger.LogInformation(
				"Обработка завершена. Всего задач: {Total}. Возобновлено: {Success}. Ошибок: {Errors}",
				taskIds.Count,
				successCount,
				errorCount);
		}

		private async Task<bool> TryResumeTask(
			IUnitOfWork uow,
			OrderEdoTask edoTask,
			CancellationToken cancellationToken)
		{
			if(!_selfDeliveryPaidValidator.IsApplicable(edoTask))
			{
				_logger.LogError(
					"Задача ЭДО {EdoTaskId} не подходит для обработки проблемой оплаты при самовывозе",
					edoTask.Id);
				return false;
			}

			var validationResult = await _selfDeliveryPaidValidator.ValidateAsync(edoTask, _serviceProvider, cancellationToken);

			if(!validationResult.IsValid)
			{
				var notificationPublished = await _notificationService.NotifyAsync(
					uow,
					edoTask,
					EdoNotificationType.OrderSelfDeliveryPaymentProblem,
					_selfDeliveryPaidValidator,
					cancellationToken);

				if(notificationPublished)
				{
					await uow.CommitAsync(cancellationToken);
				}

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

			await _messageService.PublishTaskCreatedEvent(edoTask, cancellationToken);

			return true;
		}
	}
}
