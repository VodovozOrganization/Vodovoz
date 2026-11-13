using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Services.ReceiptFailedProblem;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Problem.Routine.Worker.Consumers
{
	/// <summary>
	/// Обработчик (Consumer) события запроса переобработки чеков с ошибками фискализации
	/// </summary>
	public class ReceiptFailedReprocessingConsumer : IConsumer<ReceiptFailedReprocessingRequestedEvent>
	{
		private readonly ILogger<ReceiptFailedReprocessingConsumer> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IReceiptReprocessingLockCoordinator _lockCoordinator;

		/// <summary>
		/// Конструктор обработчика события переобработки
		/// </summary>
		/// <param name="logger">Логгер</param>
		/// <param name="serviceScopeFactory">Фабрика областей видимости служб</param>
		/// <param name="lockCoordinator">Координатор блокировки параллельного выполнения</param>
		public ReceiptFailedReprocessingConsumer(
			ILogger<ReceiptFailedReprocessingConsumer> logger,
			IServiceScopeFactory serviceScopeFactory,
			IReceiptReprocessingLockCoordinator lockCoordinator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_lockCoordinator = lockCoordinator ?? throw new ArgumentNullException(nameof(lockCoordinator));
		}

		/// <summary>
		/// Обработка входящего события запроса переобработки
		/// </summary>
		/// <param name="context">Контекст сообщения</param>
		public async Task Consume(ConsumeContext<ReceiptFailedReprocessingRequestedEvent> context)
		{
			_logger.LogInformation(
				"Получен запрос на переобработку чеков со статусом Failed от пользователя {Initiator}",
				context.Message.Initiator);

			if(!await _lockCoordinator.TryAcquireLockAsync(context.CancellationToken))
			{
				_logger.LogWarning(
					"Запрос на внеплановую переобработку чеков от пользователя {Initiator} пропущен: переобработка уже выполняется другим процессом или плановым воркером",
					context.Message.Initiator);
				return;
			}

			try
			{
				using var scope = _serviceScopeFactory.CreateScope();
				var reprocessingService = scope.ServiceProvider.GetRequiredService<IReceiptFailedReprocessingService>();

				var result = await reprocessingService.ProcessFailedDocumentsAsync(context.CancellationToken);
				_logger.LogInformation(
					"Внеплановая переобработка чеков завершена: найдено {Total}, успешно {Success}, заменено кодов {Codes}, ошибок {Errors}",
					result.TotalFound, result.SuccessCount, result.ReplacedCodesCount, result.ErrorCount);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при выполнении внеплановой переобработки чеков по запросу от {Initiator}", context.Message.Initiator);
				// Пробрасываем исключение, чтобы MassTransit зафиксировал сбой, активировал повторы и при исчерпании лимита отправил сообщение в error-очередь
				throw;
			}
			finally
			{
				await _lockCoordinator.ReleaseLockAsync();
			}
		}
	}
}
