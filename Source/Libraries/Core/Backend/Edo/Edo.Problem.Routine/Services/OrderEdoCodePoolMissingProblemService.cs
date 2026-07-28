using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using Edo.Problems.Validation;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Data.Repositories.Goods;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;

namespace Edo.Problem.Routine.Services
{
	public class OrderEdoCodePoolMissingProblemService : IOrderEdoCodePoolMissingProblemService
	{
		private readonly string _problemSourceName;
		private readonly ILogger<OrderEdoCodePoolMissingProblemService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly IBus _messageBus;
		private readonly IEdoTaskValidator _edoCodePoolValidator;
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _notificationPublisher;
		private readonly IEdoNotificationMessageFactory _notificationMessageFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEdoRepository _edoRepository;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly CodePoolMissingProblemWorkerOptions _options;

		public OrderEdoCodePoolMissingProblemService(
			ILogger<OrderEdoCodePoolMissingProblemService> logger,
			IEnumerable<IEdoTaskValidator> validators,
			IServiceProvider serviceProvider,
			IBus messageBus,
			IOutboxNotificationPublisher<EdoNotificationMessage> notificationPublisher,
			IEdoNotificationMessageFactory notificationMessageFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEdoRepository edoRepository,
			INomenclatureRepository nomenclatureRepository,
			IOptions<CodePoolMissingProblemWorkerOptions> options
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoCodePoolValidator = (validators ?? throw new ArgumentNullException(nameof(validators)))
				.FirstOrDefault(v => v.Name == _problemSourceName);
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
			_notificationMessageFactory = notificationMessageFactory ?? throw new ArgumentNullException(nameof(notificationMessageFactory));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));

			_problemSourceName = nameof(EdoCodePoolMissingCodeException);
		}

		public async Task TryResumeTaskAsync(OrderEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(_edoCodePoolValidator != null)
			{
				var validationResult = await _edoCodePoolValidator.ValidateAsync(edoTask, _serviceProvider, cancellationToken);

				if(!validationResult.IsValid)
				{
					_logger.LogDebug(
						"Задача ЭДО {EdoTaskId}: пул кодов не прошел проверку по заказу №{OrderId}",
						edoTask.Id,
						edoTask.FormalEdoRequest.Order.Id);

					throw new ArgumentException(
						$"Задача ЭДО {edoTask.Id}: пул кодов не прошел проверку по заказу №{edoTask.FormalEdoRequest.Order.Id}");
				}
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId}: пул кодов прошел проверку по заказу №{OrderId}",
				edoTask.Id,
				edoTask.FormalEdoRequest.Order.Id);

			await PublishResumeEvent(edoTask, cancellationToken);
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
					throw new ArgumentOutOfRangeException(
						$"Задача ЭДО {edoTask.Id}: неизвестный тип задачи {edoTask.GetType().Name}, не удалось определить событие для возобновления");
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

				throw new ArgumentException(
					$"Задача ЭДО {edoTask.Id} (DocumentEdoTask) находится на стадии {edoTask.Stage}. Возобновление возможно только на стадии New");
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

				throw new ArgumentException(
					$"Задача ЭДО {edoTask.Id} (TenderEdoTask) находится на стадии {edoTask.Stage}. Возобновление возможно только на стадии New");
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

				throw new ArgumentException(
					$"Задача ЭДО {edoTask.Id} (ReceiptEdoTask) находится в статусе {edoTask.ReceiptStatus}. Возобновление возможно только на стадии New");
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId} (ReceiptEdoTask) находится в статусе {ReceiptStatus}. Публикуем событие {EventName}",
				edoTask.Id,
				edoTask.ReceiptStatus,
				nameof(ReceiptTaskCreatedEvent));

			await _messageBus.Publish(new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = edoTask.Id }, cancellationToken);
		}


		public async Task ProcessProblemTasks(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				_logger.LogInformation("Начинаем поиск проблем ЭДО с ошибкой нехватки кодов в пуле");

				var problems = await _edoRepository.GetActiveProblems(uow,
					_problemSourceName,
					_options.BatchSize,
					_options.MaxAttempts,
					cancellationToken);

				if(!problems.Any())
				{
					_logger.LogDebug("Нет активных проблем ЭДО с ошибкой нехватки кодов для обработки");
					return;
				}

				_logger.LogInformation("Найдено {Count} активных проблем ЭДО с ошибкой нехватки кодов", problems.Count);

				var processedCount = 0;
				var failedCount = 0;
				var notifiedCount = 0;

				foreach(var problem in problems)
				{
					try
					{
						var edoTask = await _edoRepository.GetOrderEdoTaskById(uow, problem.EdoTask.Id, cancellationToken);

						if(edoTask is null)
						{
							_logger.LogWarning(
								"Проблема {ProblemId}: связанная задача ЭДО не является OrderEdoTask или не найдена",
								problem.Id);
							continue;
						}

						var currentAttempt = (problem.Attempts ?? 0) + 1;
						problem.Attempts = currentAttempt;

						_logger.LogDebug(
							"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: попытка обработки #{Attempt}, заказ №{OrderId}",
							problem.Id,
							edoTask.Id,
							currentAttempt,
							edoTask.FormalEdoRequest?.Order?.Id);

						await TryResumeTaskAsync(edoTask, cancellationToken);

						problem.State = TaskProblemState.Solved;
						problem.Attempts = 0;
						await uow.CommitAsync(cancellationToken);

						processedCount++;
						_logger.LogInformation(
							"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: успешно обработана, проблема закрыта",
							problem.Id,
							edoTask.Id);
					}
					catch(ArgumentException ex)
					{
						var currentAttempt = problem.Attempts ?? 0;

						var edoTask = await _edoRepository.GetOrderEdoTaskById(uow, problem.EdoTask.Id, cancellationToken);

						_logger.LogWarning(ex,
							"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: проверка не пройдена, попытка #{Attempt}",
							problem.Id,
							edoTask.Id,
							currentAttempt);

						var maxAttempts = _options.MaxAttempts;

						if(currentAttempt >= maxAttempts)
						{
							_logger.LogError(
								"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: превышен лимит попыток ({MaxAttempts}), отправляем уведомление",
								problem.Id,
								edoTask.Id,
								maxAttempts);

							var notificationSent = await TryNotifyAsync(
								uow,
								problem,
								cancellationToken);

							if(notificationSent)
							{
								notifiedCount++;
								_logger.LogInformation(
									"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: уведомление отправлено успешно",
									problem.Id,
									edoTask.Id);
							}
							else
							{
								_logger.LogWarning(
									"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: не удалось отправить уведомление",
									problem.Id,
									edoTask.Id);
							}

							problem.State = TaskProblemState.Solved;
							problem.Attempts = 0;
						}

						failedCount++;
						await uow.CommitAsync(cancellationToken);
					}
					catch(Exception ex)
					{
						failedCount++;
						var taskId = (problem.EdoTask as OrderEdoTask)?.Id ?? problem.EdoTask?.Id ?? 0;
						_logger.LogError(ex,
							"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: неожиданная ошибка при обработке",
							problem.Id,
							taskId);
					}
				}

				_logger.LogInformation(
					"Обработка проблем ЭДО с ошибкой нехватки кодов завершена: " +
					"Обработано {ProcessedCount}, Ошибок {FailedCount}, Уведомлений {NotifiedCount}",
					processedCount,
					failedCount,
					notifiedCount);
			}
		}

		public async Task<bool> TryNotifyAsync(
			IUnitOfWork uow,
			ExceptionEdoTaskProblem problem,
			CancellationToken cancellationToken)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(problem is null)
			{
				throw new ArgumentNullException(nameof(problem));
			}

			var (gtin, nomenclatureName) = await GetGtinAndNomenclature(uow, problem, cancellationToken);

			var notification = _notificationMessageFactory.Create(
				EdoNotificationType.CodePoolMissingProblem,
				("Gtin", gtin ?? "не указан"),
				("NomenclatureName", nomenclatureName ?? "не указана"));

			return await _notificationPublisher.TryPublishAsync(uow, notification, cancellationToken);
		}

		private async Task<(string Gtin, string NomenclatureName)> GetGtinAndNomenclature(
			IUnitOfWork uow,
			ExceptionEdoTaskProblem problem,
			CancellationToken cancellationToken)
		{
			try
			{
				var exceptionMessage = problem.ExceptionMessage;

				if(string.IsNullOrWhiteSpace(exceptionMessage))
				{
					_logger.LogWarning(
						"Проблема {ProblemId}: не найдено сообщение исключения для парсинга GTIN",
						problem.Id);
					return (null, null);
				}

				var gtinNumbers = ParseGtinsFromExceptionMessage(exceptionMessage);

				if(!gtinNumbers.Any())
				{
					_logger.LogWarning(
						"Проблема {ProblemId}: не найдены GTIN в сообщении исключения: {ExceptionMessage}",
						problem.Id,
						exceptionMessage);
					return (null, null);
				}

				var firstGtin = gtinNumbers.First();

				string nomenclatureName = null;

				if(string.IsNullOrEmpty(nomenclatureName))
				{
					var nomenclature = await _nomenclatureRepository.GetNomenclatureByGtinAsync(uow, firstGtin, cancellationToken);

					if(nomenclature != null)
					{
						nomenclatureName = nomenclature.Name;
					}
					else
					{
						_logger.LogWarning(
							"Проблема {ProblemId}: номенклатура не найдена по GTIN {Gtin}",
							problem.Id,
							firstGtin);
					}
				}

				var resultGtin = string.Join(", ", gtinNumbers);

				_logger.LogDebug(
					"Проблема {ProblemId}: извлечены GTIN: {Gtins}, номенклатура: {Nomenclature}",
					problem.Id,
					resultGtin,
					nomenclatureName ?? "не найдена");

				return (resultGtin, nomenclatureName);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Проблема {ProblemId}: ошибка при получении GTIN и номенклатуры",
					problem.Id);
				return (null, null);
			}
		}

		private List<string> ParseGtinsFromExceptionMessage(string exceptionMessage)
		{
			var gtins = new List<string>();

			try
			{
				var patterns = new[]
				{
					@"GTIN:\s*([\d,\s]+)",
					@"GTINs:\s*([\d,\s]+)",
					@"код для следующих GTIN:\s*([\d,\s]+)",
					@"для следующих GTIN:\s*([\d,\s]+)",
					@"GTIN\s+([\d,\s]+)"
				};

				foreach(var pattern in patterns)
				{
					var match = System.Text.RegularExpressions.Regex.Match(exceptionMessage, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
					if(match.Success && match.Groups.Count > 1)
					{
						var gtinString = match.Groups[1].Value;
						var parsed = gtinString
							.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
							.Where(s => s.Length >= 14 && s.All(char.IsDigit))
							.ToList();

						if(parsed.Any())
						{
							gtins.AddRange(parsed);
							break;
						}
					}
				}

				if(!gtins.Any())
				{
					var digitPattern = @"\b\d{14}\b";
					var matches = System.Text.RegularExpressions.Regex.Matches(exceptionMessage, digitPattern);
					foreach(System.Text.RegularExpressions.Match match in matches)
					{
						if(match.Success)
						{
							gtins.Add(match.Value);
						}
					}
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при парсинге GTIN из сообщения: {ExceptionMessage}", exceptionMessage);
			}

			return gtins.Distinct().ToList();
		}
	}
}
