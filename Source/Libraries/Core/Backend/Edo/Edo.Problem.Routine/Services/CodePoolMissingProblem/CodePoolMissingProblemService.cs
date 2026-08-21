using Edo.Problem.Routine.Options;
using Edo.Transport;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Data.Repositories.Goods;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services.CodePoolMissingProblem
{
	public class CodePoolMissingProblemService : ICodePoolMissingProblemService
	{
		private readonly string _problemSourceName;
		private readonly ILogger<CodePoolMissingProblemService> _logger;
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _notificationPublisher;
		private readonly IEdoNotificationMessageFactory _notificationMessageFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEdoRepository _edoRepository;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly MessageService _messageService;
		private readonly CodePoolMissingProblemWorkerOptions _options;

		public CodePoolMissingProblemService(
			ILogger<CodePoolMissingProblemService> logger,
			IOutboxNotificationPublisher<EdoNotificationMessage> notificationPublisher,
			IEdoNotificationMessageFactory notificationMessageFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEdoRepository edoRepository,
			INomenclatureRepository nomenclatureRepository,
			MessageService messageService,
			IOptions<CodePoolMissingProblemWorkerOptions> options
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
			_notificationMessageFactory = notificationMessageFactory ?? throw new ArgumentNullException(nameof(notificationMessageFactory));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));

			_problemSourceName = nameof(EdoCodePoolMissingCodeException);
		}

		public async Task TryResumeTaskAsync(OrderEdoTask edoTask, CancellationToken cancellationToken)
		{
			await _messageService.PublishTaskCreatedEvent(edoTask, cancellationToken);
		}

		public async Task ProcessProblemTasks(CancellationToken cancellationToken)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
				{
					_logger.LogInformation("Начинаем поиск проблем ЭДО с ошибкой нехватки кодов в пуле");

					var problemNodes = await _edoRepository.GetCodePoolMissingProblemNodes(
						uow,
						_problemSourceName,
						_options.BatchSize,
						_options.RetryIntervalHours,
						cancellationToken);

					if(!problemNodes.Any())
					{
						_logger.LogDebug("Нет активных проблем ЭДО с ошибкой нехватки кодов для обработки");
						return;
					}

					_logger.LogInformation("Найдено {Count} активных проблем ЭДО с ошибкой нехватки кодов", problemNodes.Count);

					var processedCount = 0;
					var failedCount = 0;
					var notificationsToSend = new List<CodePoolMissingProblemNotificationData>();

					foreach(var problemNode in problemNodes)
					{
						try
						{
							cancellationToken.ThrowIfCancellationRequested();

							var result = await ProcessProblem(uow, problemNode, cancellationToken);

							if(result.Processed)
							{
								processedCount++;
							}

							if(result.ShouldNotify)
							{
								notificationsToSend.Add(result.NotificationData);
							}
						}
						catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
						{
							throw;
						}
						catch(Exception ex)
						{
							failedCount++;
							_logger.LogError(ex,
								"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: ошибка при обработке",
								problemNode.Problem.Id,
								problemNode.EdoTask?.Id ?? 0);
						}
					}

					if(notificationsToSend.Any())
					{
						await SendGroupNotificationsAsync(uow, notificationsToSend, cancellationToken);
					}

					if(uow.HasChanges)
					{
						await uow.CommitAsync(cancellationToken);
					}

					_logger.LogInformation(
						"Обработка проблем ЭДО с ошибкой нехватки кодов завершена: " +
						"Обработано {ProcessedCount}, Ошибок {FailedCount}, Уведомлений {NotifiedCount}",
						processedCount,
						failedCount,
						notificationsToSend.Count);
				}
			}
			catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
			{
				_logger.LogInformation("Обработка проблем ЭДО с ошибкой нехватки кодов была отменена");
				throw;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Возникла непредвиденная ошибка в сервисе {ServiceName}",
					nameof(CodePoolMissingProblemService));
				throw;
			}
		}

		private async Task<CodePoolMissingProblemProcessResult> ProcessProblem(
			IUnitOfWork uow,
			CodePoolMissingProblemNode problemNode,
			CancellationToken cancellationToken)
		{
			var problem = problemNode.Problem;
			var edoTask = problemNode.EdoTask;
			var state = problemNode.RoutineState ?? new EdoTaskProblemRoutineState { Problem = problem };
			var now = DateTime.Now;

			if(!CodePoolMissingProblemProcessingPolicy.CanRetry(
				state,
				now,
				_options.WorkerInterval))
			{
				_logger.LogDebug(
					"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: повторная обработка уже запускалась {LastRetryTime}. Следующая попытка через {WorkerInterval}",
					problem.Id,
					edoTask?.Id ?? 0,
					state.LastRetryTime,
					_options.WorkerInterval);

				return CodePoolMissingProblemProcessResult.Empty;
			}

			state.RetryCount++;
			state.LastRetryTime = now;
			await uow.SaveAsync(state, cancellationToken: cancellationToken);

			try
			{
				await TryResumeTaskAsync(edoTask, cancellationToken);

				if(CodePoolMissingProblemProcessingPolicy.ShouldRequestNotification(
					state,
					_options.MaxAttempts))
				{
					return await PrepareNotificationData(uow, problem, edoTask, cancellationToken);
				}

				return new CodePoolMissingProblemProcessResult(true, false, null);
			}
			catch(Exception ex)
			{
				_logger.LogWarning(ex,
					"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: ошибка при обработке, попытка #{RetryCount}",
					problem.Id,
					edoTask?.Id ?? 0,
					state.RetryCount);

				return new CodePoolMissingProblemProcessResult(false, false, null);
			}
		}

		private async Task<CodePoolMissingProblemProcessResult> PrepareNotificationData(
			IUnitOfWork uow,
			ExceptionEdoTaskProblem problem,
			OrderEdoTask edoTask,
			CancellationToken cancellationToken)
		{
			_logger.LogError(
				"Проблема {ProblemId}, задача ЭДО {EdoTaskId}: достигнуто максимальное количество попыток ({MaxAttempts}), подготовка данных для уведомления",
				problem.Id,
				edoTask?.Id ?? 0,
				_options.MaxAttempts);

			var orderId = edoTask?.FormalEdoRequest?.Order?.Id ?? 0;
			var (gtin, nomenclatureName) = await GetGtinAndNomenclature(uow, problem, cancellationToken);

			var notificationData = new CodePoolMissingProblemNotificationData(
				orderId: orderId,
				gtin: gtin,
				nomenclatureName: nomenclatureName,
				problemId: problem.Id);

			return new CodePoolMissingProblemProcessResult(
				processed: false,
				shouldNotify: true,
				notificationData: notificationData);
		}

		private async Task SendGroupNotificationsAsync(
			IUnitOfWork uow,
			List<CodePoolMissingProblemNotificationData> notifications,
			CancellationToken cancellationToken)
		{
			if(!notifications.Any())
			{
				return;
			}

			try
			{
				var gtinGroups = notifications
					.Where(n => !string.IsNullOrEmpty(n.Gtin))
					.GroupBy(n => n.Gtin)
					.Select(g => new
					{
						Gtin = g.Key,
						NomenclatureName = g.First().NomenclatureName ?? "неизвестная номенклатура",
						OrderIds = g.Select(n => n.OrderId).Where(id => id > 0).Distinct().OrderBy(id => id).ToList(),
						TotalCount = g.Count()
					})
					.ToList();

				if(!gtinGroups.Any())
				{
					_logger.LogWarning("Нет данных с GTIN для формирования сводки уведомления");
					return;
				}

				var messageBuilder = new StringBuilder();
				messageBuilder.AppendLine();

				foreach(var group in gtinGroups)
				{
					var nomenclatureInfo = group.NomenclatureName;
					var ordersInfo = group.OrderIds.Any()
						? $" (заказы: {string.Join(", ", group.OrderIds)})"
						: " (заказ не указан)";

					messageBuilder.AppendLine($"- GTIN: {group.Gtin}, номенклатура: \"{nomenclatureInfo}\", количество проблем: {group.TotalCount}{ordersInfo}");
				}

				var message = messageBuilder.ToString();

				var notification = _notificationMessageFactory.Create(
					EdoNotificationType.CodePoolMissingProblem,
					("Message", message));

				var published = await _notificationPublisher.TryPublishAsync(uow, notification, cancellationToken);

				if(published)
				{
					_logger.LogInformation(
						"Отправлено сводное уведомление по {ProblemCount} проблемам, затронуто GTIN: {GtinCount}",
						notifications.Count,
						gtinGroups.Count);
				}
				else
				{
					_logger.LogWarning("Не удалось отправить сводное уведомление по {ProblemCount} проблемам", notifications.Count);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке сводного уведомления");
				throw;
			}
		}

		private async Task<(string Gtin, string NomenclatureName)> GetGtinAndNomenclature(
			IUnitOfWork uow,
			ExceptionEdoTaskProblem problem,
			CancellationToken cancellationToken)
		{
			try
			{
				string gtin = null;
				string nomenclatureName = null;

				if(problem.CustomItems?.Any() == true)
				{
					var gtinItem = problem.CustomItems
						.OfType<EdoProblemGtinItem>()
						.FirstOrDefault();

					if(gtinItem?.Gtin != null)
					{
						gtin = gtinItem.Gtin.GtinNumber;
						_logger.LogDebug(
							"Проблема {ProblemId}: найден GTIN в CustomItems: {Gtin}",
							problem.Id,
							gtin);
					}
				}

				if(string.IsNullOrEmpty(gtin))
				{
					_logger.LogWarning(
						"Проблема {ProblemId}: не удалось получить GTIN",
						problem.Id);
					return (null, null);
				}

				var nomenclature = await _nomenclatureRepository.GetNomenclatureByGtinAsync(uow, gtin, cancellationToken);

				if(nomenclature != null)
				{
					nomenclatureName = nomenclature.Name;
					_logger.LogDebug(
						"Проблема {ProblemId}: найдена номенклатура {Nomenclature} по GTIN {Gtin}",
						problem.Id,
						nomenclatureName,
						gtin);
				}
				else
				{
					_logger.LogWarning(
						"Проблема {ProblemId}: номенклатура не найдена по GTIN {Gtin}",
						problem.Id,
						gtin);
				}

				return (gtin, nomenclatureName);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Проблема {ProblemId}: ошибка при получении GTIN и номенклатуры",
					problem.Id);
				return (null, null);
			}
		}
	}
}
