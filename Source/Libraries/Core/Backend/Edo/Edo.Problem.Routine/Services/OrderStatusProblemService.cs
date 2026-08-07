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
	/// Сервис обработки проблем с неверным статусом заказа в ЭДО
	/// </summary>
	public class OrderStatusProblemService
	{
		private const string _problemSourceName = "Order.Status";

		private readonly ILogger<OrderStatusProblemService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptionsMonitor<OrderStatusProblemWorkerOptions> _options;
		private readonly IEdoTaskValidator _orderStatusValidator;
		private readonly IServiceProvider _serviceProvider;
		private readonly IEdoRepository _edoRepository;
		private readonly IBus _messageBus;
		private readonly IEdoProblemRoutineNotificationService _notificationService;
		private readonly MessageService _messageService;

		public OrderStatusProblemService(
			ILogger<OrderStatusProblemService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptionsMonitor<OrderStatusProblemWorkerOptions> options,
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
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

			_orderStatusValidator = (validators ?? throw new ArgumentNullException(nameof(validators)))
				.FirstOrDefault(v => v.Name == _problemSourceName)
				?? throw new InvalidOperationException($"Валидатор с именем '{_problemSourceName}' не зарегистрирован");
			_messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
		}

		private DateTime _minEdoTaskCreationTime => DateTime.Today - _options.CurrentValue.ProblemTimeout;

		/// <summary>
		/// Обработчик задач с неверным статусом заказа в ЭДО
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		public async Task ProcessProblemTasks(CancellationToken cancellationToken)
		{
			IList<int> taskIds;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(OrderStatusProblemService)))
			{
				var tasks = await _edoRepository.GetProblemEdoTasks(uow, _problemSourceName, _minEdoTaskCreationTime, cancellationToken);

				_logger.LogInformation("Найдено {Count} задач ЭДО с активной проблемой {ProblemName}",
					tasks.Count, _problemSourceName);

				taskIds = tasks.Select(x => x.Id).ToList();
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
					using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(OrderStatusProblemService)))
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
				var notificationPublished = await _notificationService.NotifyAsync(
					uow,
					edoTask,
					EdoNotificationType.OrderStatusProblem,
					_orderStatusValidator,
					cancellationToken);

				if(notificationPublished)
				{
					await uow.CommitAsync(cancellationToken);
				}

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

			await _messageService.PublishTaskCreatedEvent(edoTask, cancellationToken);

			return true;
		}
	}
}
