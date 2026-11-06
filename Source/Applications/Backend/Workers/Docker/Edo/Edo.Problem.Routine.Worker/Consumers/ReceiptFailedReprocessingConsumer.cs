using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Services.ReceiptFailedProblem;
using MassTransit;
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
		private readonly IReceiptFailedReprocessingService _reprocessingService;

		/// <summary>
		/// Конструктор обработчика события переобработки
		/// </summary>
		/// <param name="logger">Логгер</param>
		/// <param name="reprocessingService">Сервис переобработки чеков со статусом Failed</param>
		public ReceiptFailedReprocessingConsumer(
			ILogger<ReceiptFailedReprocessingConsumer> logger,
			IReceiptFailedReprocessingService reprocessingService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_reprocessingService = reprocessingService ?? throw new ArgumentNullException(nameof(reprocessingService));
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

			if(!ReceiptFailedReprocessingWorker.TryAcquireRunningLock())
			{
				_logger.LogInformation("Запрос на переобработку отклонен: переобработка чеков уже выполняется");
				return;
			}

			try
			{
				var result = await _reprocessingService.ProcessFailedDocumentsAsync(context.CancellationToken);
				_logger.LogInformation(
					"Переобработка чеков завершена: найдено {Total}, успешно {Success}, заменено кодов {Codes}, ошибок {Errors}",
					result.TotalFound, result.SuccessCount, result.ReplacedCodesCount, result.ErrorCount);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при выполнении переобработки чеков с ошибками");
			}
			finally
			{
				ReceiptFailedReprocessingWorker.ReleaseRunningLock();
			}
		}
	}
}
