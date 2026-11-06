using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Edo.Problem.Routine.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Rules.Edo;

namespace Edo.Problem.Routine.Services.ReceiptFailedProblem
{
	/// <summary>
	/// Сервис мониторинга, проверки и автоматической переотправки чеков с ошибкой фискализации
	/// </summary>
	public class ReceiptFailedReprocessingService : IReceiptFailedReprocessingService
	{
		private readonly ILogger<ReceiptFailedReprocessingService> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IOptionsMonitor<ReceiptFailedReprocessingOptions> _options;
		private readonly IEdoRepository _edoRepository;
		private readonly CanProcessOrSendEdoByClosingAccountingDate _canProcessOrSendEdo;

		/// <summary>
		/// Конструктор сервиса переобработки чеков с ошибками фискализации
		/// </summary>
		/// <param name="logger">Логгер</param>
		/// <param name="serviceScopeFactory">Фабрика областей видимости зависимостей (Scope Factory)</param>
		/// <param name="options">Монитор настроек переобработки</param>
		/// <param name="edoRepository">Репозиторий сущностей ЭДО</param>
		/// <param name="canProcessOrSendEdo">Правило проверки закрытого бухгалтерского периода</param>
		public ReceiptFailedReprocessingService(
			ILogger<ReceiptFailedReprocessingService> logger,
			IServiceScopeFactory serviceScopeFactory,
			IOptionsMonitor<ReceiptFailedReprocessingOptions> options,
			IEdoRepository edoRepository,
			CanProcessOrSendEdoByClosingAccountingDate canProcessOrSendEdo)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_canProcessOrSendEdo = canProcessOrSendEdo ?? throw new ArgumentNullException(nameof(canProcessOrSendEdo));
		}

		/// <inheritdoc/>
		public async Task<ReprocessingExecutionResult> ProcessFailedDocumentsAsync(CancellationToken cancellationToken)
		{
			var executionResult = new ReprocessingExecutionResult();
			var lookbackDays = _options.CurrentValue.LookbackDays;
			var failedOlderThanHours = _options.CurrentValue.FailedOlderThanHours;

			_logger.LogInformation(
				"Старт обработки чеков со статусом Failed за последние {LookbackDays} дн. (ошибка старше {FailedHours} ч.)",
				lookbackDays,
				failedOlderThanHours);

			var failedOlderThan = DateTime.Now.AddHours(-failedOlderThanHours);
			var lookbackFrom = DateTime.Today.AddDays(-lookbackDays);
			var closedPeriodDate = _canProcessOrSendEdo.GetCheckingDate();

			var effectiveStartDate = closedPeriodDate > lookbackFrom
				? closedPeriodDate
				: lookbackFrom;

			IList<int> failedDocumentIds;
			using(var queryScope = _serviceScopeFactory.CreateScope())
			{
				var uow = queryScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
				failedDocumentIds = await _edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
					uow,
					statusChangedBefore: failedOlderThan,
					statusChangedNotBefore: effectiveStartDate,
					cancellationToken: cancellationToken);
			}

			executionResult.TotalFound = failedDocumentIds.Count;

			if(failedDocumentIds.Count == 0)
			{
				_logger.LogInformation("Не найдено документов со статусом Failed для переобработки");
				return executionResult;
			}

			_logger.LogInformation("Найдено {Count} документов со статусом Failed для переобработки", failedDocumentIds.Count);

			foreach(var docId in failedDocumentIds)
			{
				if(cancellationToken.IsCancellationRequested)
				{
					_logger.LogInformation("Переобработка чеков отменена по CancellationToken");
					break;
				}

				try
				{
					var docResult = await ProcessSingleDocumentAsync(docId, cancellationToken);

					executionResult.SuccessCount += docResult.IsSuccess ? 1 : 0;
					executionResult.ErrorCount += docResult.IsError ? 1 : 0;
					executionResult.SkippedCount += docResult.IsSkipped ? 1 : 0;
					executionResult.ReplacedCodesCount += docResult.ReplacedCodesCount;
				}
				catch(OperationCanceledException)
				{
					_logger.LogInformation("Обработка документов была отменена");
					break;
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Непредвиденная ошибка при обработке документа {DocId}", docId);
					executionResult.ErrorCount++;
				}
			}

			_logger.LogInformation(
				"Обработка завершена. Всего: {Total}, успешно: {Success}, заменено кодов: {Codes}, пропущено (в кассе/сбой): {Skipped}, ошибок: {Errors}",
				executionResult.TotalFound,
				executionResult.SuccessCount,
				executionResult.ReplacedCodesCount,
				executionResult.SkippedCount,
				executionResult.ErrorCount);

			return executionResult;
		}

		/// <inheritdoc/>
		public async Task<FiscalDocumentReprocessingResult> ProcessSingleDocumentAsync(
			int fiscalDocumentId,
			CancellationToken cancellationToken)
		{
			using(var scope = _serviceScopeFactory.CreateScope())
			{
				var processor = scope.ServiceProvider.GetRequiredService<IReceiptFailedDocumentReprocessingProcessor>();
				return await processor.ProcessDocumentAsync(fiscalDocumentId, cancellationToken);
			}
		}
	}
}
