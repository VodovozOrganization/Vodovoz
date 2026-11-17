using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Edo.Receipt;
using Edo.Receipt.Sender;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModulKassa;
using ModulKassa.DTO;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using EdoFiscalStatus = Vodovoz.Core.Domain.Edo.FiscalDocumentStatus;
using ModulFiscalStatus = ModulKassa.DTO.FiscalDocumentStatus;

namespace Edo.Problem.Routine.Services.ReceiptFailedProblem
{
	/// <summary>
	/// Процессор обработки отдельного фискального чека со статусом Failed в изолированной транзакции и области видимости (Scope)
	/// </summary>
	public class ReceiptFailedDocumentReprocessingProcessor : IReceiptFailedDocumentReprocessingProcessor
	{
		private readonly ILogger<ReceiptFailedDocumentReprocessingProcessor> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOptionsMonitor<ReceiptFailedReprocessingOptions> _options;
		private readonly IEdoRepository _edoRepository;
		private readonly CashboxClientProvider _cashboxClientProvider;
		private readonly FiscalDocumentFactory _fiscalDocumentFactory;
		private readonly ITrueMarkCodesPoolCodeProvider _trueMarkCodesPoolCodeProvider;
		private readonly ITrueMarkCodesValidator _trueMarkCodesValidator;
		private readonly IPublishEndpoint _publishEndpoint;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private ReceiptTrueMarkCodesPool _trueMarkCodesPool;

		private ReceiptTrueMarkCodesPool CodesPool =>
			_trueMarkCodesPool ?? (_trueMarkCodesPool = new ReceiptTrueMarkCodesPool(_unitOfWork));

		/// <summary>
		/// Конструктор процессора переобработки документа
		/// </summary>
		public ReceiptFailedDocumentReprocessingProcessor(
			ILogger<ReceiptFailedDocumentReprocessingProcessor> logger,
			IUnitOfWork unitOfWork,
			IUnitOfWorkFactory uowFactory,
			IOptionsMonitor<ReceiptFailedReprocessingOptions> options,
			IEdoRepository edoRepository,
			CashboxClientProvider cashboxClientProvider,
			FiscalDocumentFactory fiscalDocumentFactory,
			ITrueMarkCodesPoolCodeProvider trueMarkCodesPoolCodeProvider,
			ITrueMarkCodesValidator trueMarkCodesValidator,
			IPublishEndpoint publishEndpoint,
			EdoProblemRegistrar edoProblemRegistrar)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
			_fiscalDocumentFactory = fiscalDocumentFactory ?? throw new ArgumentNullException(nameof(fiscalDocumentFactory));
			_trueMarkCodesPoolCodeProvider = trueMarkCodesPoolCodeProvider ?? throw new ArgumentNullException(nameof(trueMarkCodesPoolCodeProvider));
			_trueMarkCodesValidator = trueMarkCodesValidator ?? throw new ArgumentNullException(nameof(trueMarkCodesValidator));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
		}

		/// <inheritdoc/>
		public async Task<FiscalDocumentReprocessingResult> ProcessDocumentAsync(int fiscalDocumentId, CancellationToken cancellationToken)
		{
			var edoFiscalDocument = _unitOfWork.GetById<EdoFiscalDocument>(fiscalDocumentId);
			if(edoFiscalDocument == null)
			{
				_logger.LogWarning("Фискальный документ с Id {DocId} не найден в БД", fiscalDocumentId);

				return FiscalDocumentReprocessingResult.Error();
			}

			var receiptEdoTask = edoFiscalDocument.ReceiptEdoTask;
			if(receiptEdoTask == null)
			{
				_logger.LogWarning("Для документа {DocId} не найдена связанная задача ReceiptEdoTask", fiscalDocumentId);

				return FiscalDocumentReprocessingResult.Error();
			}

			if(!receiptEdoTask.CashboxId.HasValue)
			{
				_logger.LogWarning("В задаче {TaskId} документа {DocId} не указан CashboxId кассы", receiptEdoTask.Id, fiscalDocumentId);

				return FiscalDocumentReprocessingResult.Error();
			}

			var documentGuid = edoFiscalDocument.DocumentGuid.ToString();

			_logger.LogInformation(
				"Проверка документа {DocId} (Guid: {Guid}, Касса: {CashboxId}) в МодульКассе",
				fiscalDocumentId, documentGuid, receiptEdoTask.CashboxId.Value);

			var cashboxClient = await _cashboxClientProvider.GetCashboxAsync(receiptEdoTask.CashboxId.Value, cancellationToken);
			var modulCheckResult = await cashboxClient.CheckFiscalDocument(documentGuid, cancellationToken);

			// Кейс 1: Чек уже фактически фискализирован в МодульКассе со статусом Completed или Printed
			if(modulCheckResult.SendStatus == SendStatus.Success
				&& modulCheckResult.FiscalDocumentInfo != null
				&& (modulCheckResult.FiscalDocumentInfo.Status == ModulFiscalStatus.Completed
					|| modulCheckResult.FiscalDocumentInfo.Status == ModulFiscalStatus.Printed))
			{
				_logger.LogInformation(
					"Документ {DocId} фактически фискализирован в МодульКассе со статусом {Status}. Обновляем статус в БД",
					fiscalDocumentId, modulCheckResult.FiscalDocumentInfo.Status);

				if(modulCheckResult.FiscalDocumentInfo.FiscalInfo == null)
				{
					_logger.LogWarning(
						"Документ {DocId}: касса вернула статус {Status}, но фискальные данные отсутствуют (FiscalInfo == null). Пропуск до подтверждения фискализации",
						fiscalDocumentId, modulCheckResult.FiscalDocumentInfo.Status);

					return FiscalDocumentReprocessingResult.Skipped();
				}

				ReceiptFiscalDocumentUpdater.UpdateFiscalDocument(edoFiscalDocument, modulCheckResult.FiscalDocumentInfo);

				await _unitOfWork.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);

				var allCompleted = receiptEdoTask.FiscalDocuments.All(x =>
					x.Id == edoFiscalDocument.Id
					? true
					: (x.Stage == FiscalDocumentStage.Completed && (x.Status == EdoFiscalStatus.Printed || x.Status == EdoFiscalStatus.Completed)));

				if(allCompleted)
				{
					await CompleteReceiptAsync(receiptEdoTask, cancellationToken);

					// Публикация в Transactional Outbox выполняется до CommitAsync, чтобы данные и строки outbox сохранились атомарно в единой транзакции NHibernate
					await _publishEndpoint.Publish(
						new ReceiptCompleteEvent { ReceiptEdoTaskId = receiptEdoTask.Id },
						cancellationToken);
				}
				else
				{
					if(receiptEdoTask.Status == EdoTaskStatus.Problem)
					{
						receiptEdoTask.Status = EdoTaskStatus.InProgress;
					}
					await _unitOfWork.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
				}

				await _unitOfWork.CommitAsync(cancellationToken);

				_logger.LogInformation(
					"Документ {DocId} успешно переведен в Completed",
					fiscalDocumentId);

				return FiscalDocumentReprocessingResult.Success();
			}

			// Кейс 2: Документ в активной обработке кассой (Queued, Pending, WaitForCallback)
			if(modulCheckResult.SendStatus == SendStatus.Success
				&& modulCheckResult.FiscalDocumentInfo != null
				&& (modulCheckResult.FiscalDocumentInfo.Status == ModulFiscalStatus.Queued
					|| modulCheckResult.FiscalDocumentInfo.Status == ModulFiscalStatus.Pending
					|| modulCheckResult.FiscalDocumentInfo.Status == ModulFiscalStatus.WaitForCallback))
			{
				_logger.LogInformation(
					"Документ {DocId} находится в процессе обработки МодульКассой (статус: {Status}). Повторная отправка отложена во избежание дубля",
					fiscalDocumentId, modulCheckResult.FiscalDocumentInfo.Status);

				return FiscalDocumentReprocessingResult.Skipped();
			}

			// Кейс 3: Ошибка запроса к кассе (кроме явного 404/NotFound)
			if(modulCheckResult.SendStatus == SendStatus.Error && !IsDocumentNotFound(modulCheckResult.ErrorMessage))
			{
				_logger.LogWarning(
					"Не удалось проверить статус документа {DocId} в МодульКассе ({Message}). Регистрируем проблему и откладываем повторную отправку",
					fiscalDocumentId, modulCheckResult.ErrorMessage);
				await HandleFailureInCleanSessionAsync(
					fiscalDocumentId,
					receiptEdoTask.Id,
					$"Сбой связи с МодульКассой при проверке статуса: {modulCheckResult.ErrorMessage}",
					cancellationToken);

				return FiscalDocumentReprocessingResult.Error();
			}

			// Кейс 4: Чек подтвержден как Failed или не найден в кассе (404)
			_logger.LogInformation(
				"Документ {DocId} готов к повторной отправке (статус в кассе: {ModulStatus})",
				fiscalDocumentId, modulCheckResult.FiscalDocumentInfo?.Status.ToString() ?? "NOT_FOUND");

			// Проверка допустимости автоматической повторной отправки
			var eligibility = CheckReprocessingEligibility(modulCheckResult.FiscalDocumentInfo);
			if(!eligibility.IsEligible)
			{
				_logger.LogWarning(
					"Документ {DocId} не может быть повторно отправлен автоматически: {Reason}",
					fiscalDocumentId, eligibility.BlockReason);

				await HandleFailureInCleanSessionAsync(fiscalDocumentId, receiptEdoTask.Id, eligibility.BlockReason, cancellationToken);

				return FiscalDocumentReprocessingResult.Error();
			}

			// Проверка кодов маркировки по тегу 1260 и автозамена из пула (в транзакции для гарантии атомарности)
			var codesReplaced = 0;
			try
			{
				codesReplaced = await CheckAndReplaceTrueMarkCodesAsync(edoFiscalDocument, receiptEdoTask, cancellationToken);
			}
			catch(EdoCodePoolMissingCodeException ex)
			{
				await RollbackTransactionSafelyAsync(cancellationToken);
				_unitOfWork.Session.Clear();

				_logger.LogWarning(ex, "Для документа {DocId} не удалось подобрать код из пула: {Message}", fiscalDocumentId, ex.Message);
				await HandleFailureInCleanSessionAsync(fiscalDocumentId, receiptEdoTask.Id, ex.Message, cancellationToken);

				return FiscalDocumentReprocessingResult.Error();
			}
			catch(Exception ex) when(IsNetworkOrInfrastructureException(ex))
			{
				await RollbackTransactionSafelyAsync(cancellationToken);
				_unitOfWork.Session.Clear();

				_logger.LogWarning(ex, "Инфраструктурный сбой сети при проверке кодов для документа {DocId}", fiscalDocumentId);
				await HandleFailureInCleanSessionAsync(
					fiscalDocumentId,
					receiptEdoTask.Id,
					$"Сетевой сбой при проверке кодов: {ex.Message}",
					cancellationToken);

				return FiscalDocumentReprocessingResult.Error();
			}
			catch(Exception ex)
			{
				await RollbackTransactionSafelyAsync(cancellationToken);
				_unitOfWork.Session.Clear();

				_logger.LogWarning(ex, "Ошибка подготовки кодов/реквизитов для документа {DocId}: {Message}", fiscalDocumentId, ex.Message);
				await HandleFailureInCleanSessionAsync(fiscalDocumentId, receiptEdoTask.Id, ex.Message, cancellationToken);

				return FiscalDocumentReprocessingResult.Error();
			}

			FiscalizationResult sendResult;
			var isDocumentNotFoundInCashbox = modulCheckResult.SendStatus == SendStatus.Error
				&& IsDocumentNotFound(modulCheckResult.ErrorMessage);

			var isPayloadChanged = codesReplaced > 0;

			var cashboxFailureInfo = modulCheckResult.FiscalDocumentInfo?.FailureInfo;
			var lastFailureMessage = !string.IsNullOrWhiteSpace(cashboxFailureInfo?.Type) && !string.IsNullOrWhiteSpace(cashboxFailureInfo?.Message)
				? $"{cashboxFailureInfo.Type}: {cashboxFailureInfo.Message}"
				: cashboxFailureInfo?.Message ?? cashboxFailureInfo?.Type ?? edoFiscalDocument.FailureMessage;

			await EnsureFiscalizationProblemAsync(receiptEdoTask.Id, lastFailureMessage, cancellationToken);

			if(isPayloadChanged)
			{
				// Была произведена замена кодов маркировки из пула (codesReplaced > 0).
				// В МодульКассе по старому GUID лежат старые (бракованные) коды, а re-queue не умеет обновлять состав позиций.
				// Поэтому для обновленного состава кодов генерируем новый DocumentGuid и отправляем через POST /fn/v2/doc.
				var oldGuid = edoFiscalDocument.DocumentGuid;
				var newGuid = Guid.NewGuid();
				edoFiscalDocument.DocumentGuid = newGuid;

				_logger.LogInformation(
					"Документ {DocId} задачи {TaskId}: заменено кодов маркировки из пула: {CodesCount}. Назначен новый DocumentGuid {NewGuid} (предыдущий: {OldGuid})",
					fiscalDocumentId, receiptEdoTask.Id, codesReplaced, newGuid, oldGuid);

				await _unitOfWork.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
				await _unitOfWork.CommitAsync(cancellationToken);

				var fiscalDocument = _fiscalDocumentFactory.CreateFiscalDocument(edoFiscalDocument);
				_logger.LogInformation("Повторная отправка фискального документа {DocId} (Guid: {Guid}) через POST /fn/v2/doc в МодульКассу", fiscalDocumentId, newGuid);
				sendResult = await cashboxClient.SendFiscalDocument(fiscalDocument, cancellationToken);
			}
			else if(isDocumentNotFoundInCashbox)
			{
				// Документ ранее не был зарегистрирован в МодульКассе (404 Not Found).
				// Отправляем первичный документ со штатным DocumentGuid через POST /fn/v2/doc.
				_logger.LogInformation(
					"Документ {DocId} ранее не был зарегистрирован в МодульКассе (404). Отправка через POST /fn/v2/doc (Guid: {Guid})",
					fiscalDocumentId, edoFiscalDocument.DocumentGuid);

				await _unitOfWork.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
				await _unitOfWork.CommitAsync(cancellationToken);

				var fiscalDocument = _fiscalDocumentFactory.CreateFiscalDocument(edoFiscalDocument);
				sendResult = await cashboxClient.SendFiscalDocument(fiscalDocument, cancellationToken);
			}
			else
			{
				// Коды маркировки валидны (замен кодов не производилось) либо документ без маркировки.
				// Используем штатный метод МодульКассы PUT /fn/v1/doc/{id}/re-queue для повторной постановки чека в очередь кассы.
				// Это не создает дубликатов в кассе и сохраняет исходный DocumentGuid.
				var guid = edoFiscalDocument.DocumentGuid;
				_logger.LogInformation(
					"Документ {DocId}: коды не менялись (замен: {CodesCount}). Выполняем повторное проведение через re-queue (GUID: {Guid})",
					fiscalDocumentId, codesReplaced, guid);

				await _unitOfWork.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
				await _unitOfWork.CommitAsync(cancellationToken);

				sendResult = await cashboxClient.RequeueFiscalDocument(guid.ToString(), cancellationToken);
			}

			if(sendResult.SendStatus == SendStatus.Success)
			{
				// Если касса вернула статус Failed
				if(sendResult.FiscalDocumentInfo?.Status == ModulFiscalStatus.Failed)
				{
					var failureInfo = sendResult.FiscalDocumentInfo.FailureInfo;
					var failureMsg = !string.IsNullOrWhiteSpace(failureInfo?.Type) && !string.IsNullOrWhiteSpace(failureInfo?.Message)
						? $"{failureInfo.Type}: {failureInfo.Message}"
						: failureInfo?.Message ?? failureInfo?.Type ?? sendResult.ErrorMessage ?? "Документ отклонен сервером фискализации (статус Failed)";

					_logger.LogWarning(
						"Повторная обработка документа {DocId} в МодульКассе вернула статус FAILED: {Error}",
						fiscalDocumentId, failureMsg);

					await HandleFailureInCleanSessionAsync(fiscalDocumentId, receiptEdoTask.Id, failureMsg, cancellationToken);

					return FiscalDocumentReprocessingResult.Error();
				}

				// Документ поставлен в очередь кассы (Queued / Pending)
				edoFiscalDocument.Stage = FiscalDocumentStage.Sent;
				edoFiscalDocument.Status = sendResult.FiscalDocumentInfo != null
					? ReceiptConverters.ConvertFiscalDocumentStatus(sendResult.FiscalDocumentInfo.Status)
					: EdoFiscalStatus.Queued;
				edoFiscalDocument.StatusChangeTime = DateTime.Now;

				_logger.LogInformation(
					"Документ {DocId} успешно поставлен в очередь кассы (статус: {Status}). Ожидает завершения печати кассовым аппаратом",
					fiscalDocumentId, edoFiscalDocument.Status);

				await _unitOfWork.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
				await _unitOfWork.CommitAsync(cancellationToken);

				return FiscalDocumentReprocessingResult.Success(codesReplaced);
			}
			else
			{
				_logger.LogWarning("Обработка в МодульКассе для документа {DocId} завершилась ошибкой: {Error}", fiscalDocumentId, sendResult.ErrorMessage);

				await HandleFailureInCleanSessionAsync(
					fiscalDocumentId,
					receiptEdoTask.Id,
					$"Сбой при отправке/re-queue в кассу: {sendResult.ErrorMessage}",
					cancellationToken);

				return FiscalDocumentReprocessingResult.Error();
			}
		}

		/// <summary>
		/// Проверяет маркированные позиции чека по тегу 1260 и разрешительному режиму в Честном Знаке,
		/// и при необходимости производит автозамену невалидных кодов на подходящие из пула
		/// </summary>
		private async Task<int> CheckAndReplaceTrueMarkCodesAsync(
			EdoFiscalDocument edoFiscalDocument,
			ReceiptEdoTask receiptEdoTask,
			CancellationToken cancellationToken)
		{
			var markedPositions = edoFiscalDocument.InventPositions
				.Where(x => x.EdoTaskItem?.ProductCode != null || x.GroupCode != null)
				.ToList();

			if(!markedPositions.Any())
			{
				return 0;
			}

			var seller = receiptEdoTask.FormalEdoRequest?.Order?.Contract?.Organization;
			var organizationInn = seller?.INN ?? string.Empty;

			var docCreationTime = edoFiscalDocument.CreationTime;

			var codesToCheck = markedPositions
				.Select(GetPositionIdentificationCode)
				.Where(c => !string.IsNullOrEmpty(c))
				.Distinct()
				.ToList();

			_logger.LogInformation(
				"Проверка {Count} уникальных кодов маркировки через валидатор ЧЗ для документа {DocId}",
				codesToCheck.Count, edoFiscalDocument.Id);

			var validationResult = await _trueMarkCodesValidator.ValidateAsync(
				codesToCheck,
				organizationInn,
				cancellationToken);

			var positionsNeedingNewCodes = new List<PositionCodeReplacementContext>();

			foreach(var pos in markedPositions)
			{
				var isProductCode = pos.EdoTaskItem?.ProductCode != null;
				var productCode = pos.EdoTaskItem?.ProductCode;
				var idCode = GetPositionIdentificationCode(pos);

				if(string.IsNullOrEmpty(idCode))
				{
					continue;
				}

				var codeResult = validationResult?.CodeResults?.FirstOrDefault(c => c.CodeString == idCode);
				var isValid = codeResult != null && codeResult.IsValid;

				if(isValid)
				{
					_logger.LogInformation(
						"Код маркировки для позиции {PosName} прошел проверку в Честном Знаке (валиден)",
						pos.Name);

					if(isProductCode && productCode.ResultCode == null)
					{
						productCode.ResultCode = productCode.SourceCode;
						await _unitOfWork.SaveAsync(productCode, cancellationToken: cancellationToken);
					}
				}
				else
				{
					if(!isProductCode)
					{
						throw new InvalidOperationException(
							$"Групповой код маркировки для позиции \"{pos.Name}\" не прошел проверку в Честном Знаке. Автозамена групповых кодов не поддерживается, требуется ручная обработка фин.отделом.");
					}

					positionsNeedingNewCodes.Add(new PositionCodeReplacementContext
					{
						Position = pos,
						ProductCode = productCode
					});
				}
			}

			if(!positionsNeedingNewCodes.Any())
			{
				return 0;
			}

			var reasonForLeaving = receiptEdoTask.FormalEdoRequest?.Order?.Client?.ReasonForLeaving;
			var canReplaceCodes = reasonForLeaving != ReasonForLeaving.Resale && reasonForLeaving != ReasonForLeaving.Tender;

			if(!canReplaceCodes)
			{
				throw new InvalidOperationException(
					$"Для причины выбытия \"{reasonForLeaving}\" автозамена кодов маркировки из пула запрещена. " +
					$"Позиции чека ({positionsNeedingNewCodes.Count} шт.) не прошли проверку в Честном Знаке. " +
					"Требуется ручной разбор фин. отделом / складом.");
			}

			foreach(var item in positionsNeedingNewCodes)
			{
				var gtin = item.ProductCode.ResultCode?.Gtin ?? item.ProductCode.SourceCode?.Gtin;

				_logger.LogInformation(
					"Подбор кода из пула для позиции \"{PosName}\" (GTIN: {GTIN})",
					item.Position.Name, gtin);

				var gtinEntity = await _edoRepository.GetGtinByGtinNumberAsync(gtin, cancellationToken)
					?? new GtinEntity { GtinNumber = gtin };

				var newCode = await _trueMarkCodesPoolCodeProvider.TakeValidCodeAsync(
					CodesPool,
					gtinEntity,
					organizationInn,
					cancellationToken,
					addingTimeNotAfter: docCreationTime);

				item.ProductCode.ResultCode = newCode;
				item.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;
				await _unitOfWork.SaveAsync(item.ProductCode, cancellationToken: cancellationToken);
			}

			return positionsNeedingNewCodes.Count;
		}

		private static string GetPositionIdentificationCode(FiscalInventPosition pos)
		{
			return pos.EdoTaskItem?.ProductCode != null
				? (pos.EdoTaskItem.ProductCode.ResultCode ?? pos.EdoTaskItem.ProductCode.SourceCode)?.IdentificationCode
				: pos.GroupCode?.IdentificationCode;
		}

		private async Task CompleteReceiptAsync(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			receiptEdoTask.ReceiptStatus = EdoReceiptStatus.Completed;
			receiptEdoTask.Status = EdoTaskStatus.Completed;

			_edoProblemRegistrar.SolveCustomProblem<ReceiptFiscalizationFailed>(receiptEdoTask);
			_edoProblemRegistrar.SolveCustomProblem<ReceiptSendingFailed>(receiptEdoTask);

			await _unitOfWork.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
		}

		private async Task HandleFailureInCleanSessionAsync(
			int docId,
			int receiptEdoTaskId,
			string failureReason,
			CancellationToken cancellationToken)
		{
			try
			{
				using(var failureUow = _uowFactory.CreateWithoutRoot())
				{
					var doc = failureUow.GetById<EdoFiscalDocument>(docId);
					if(doc != null)
					{
						if(doc.Status != EdoFiscalStatus.Failed)
						{
							doc.Status = EdoFiscalStatus.Failed;
							doc.StatusChangeTime = DateTime.Now;
						}
						doc.FailureMessage = failureReason;
						await failureUow.SaveAsync(doc, cancellationToken: cancellationToken);
						await failureUow.CommitAsync(cancellationToken);
					}
				}
			}
			catch(Exception ex)
			{
				_logger.LogWarning(ex, "Не удалось обновить статус ошибки в БД для документа {DocId}", docId);
			}

			if(receiptEdoTaskId > 0)
			{
				await EnsureFiscalizationProblemAsync(receiptEdoTaskId, failureReason, cancellationToken);
			}
		}

		/// <summary>
		/// Обеспечивает наличие активной проблемы ReceiptFiscalizationFailed для задачи
		/// и актуализирует CustomMessage текстом ошибки
		/// </summary>
		private async Task EnsureFiscalizationProblemAsync(
			int receiptEdoTaskId,
			string failureMessage,
			CancellationToken cancellationToken)
		{
			try
			{
				var customMsg = !string.IsNullOrWhiteSpace(failureMessage)
					? failureMessage
					: "Ошибка фискализации чека (переобработка)";

				await _edoProblemRegistrar.RegisterCustomProblem<ReceiptFiscalizationFailed>(
					receiptEdoTaskId,
					cancellationToken,
					customMessage: customMsg,
					disposeTaskUow: false);

				using(var uow = _uowFactory.CreateWithoutRoot())
				{
					var task = uow.GetById<ReceiptEdoTask>(receiptEdoTaskId);
					var activeProblem = task?.Problems?.FirstOrDefault(p =>
						p.State == TaskProblemState.Active
						&& p.SourceName != null
						&& p.SourceName.IndexOf(nameof(ReceiptFiscalizationFailed), StringComparison.OrdinalIgnoreCase) >= 0);

					if(activeProblem == null)
					{
						_logger.LogWarning("Не удалось найти или зарегистрировать проблему ReceiptFiscalizationFailed для задачи {TaskId}", receiptEdoTaskId);
						return;
					}

					if(!string.IsNullOrWhiteSpace(failureMessage)
						&& activeProblem is CustomEdoTaskProblem customProblem
						&& customProblem.CustomMessage != failureMessage)
					{
						customProblem.CustomMessage = failureMessage;
						await uow.SaveAsync(customProblem, cancellationToken: cancellationToken);
						await uow.CommitAsync(cancellationToken);
					}
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при регистрации или актуализации проблемы для задачи {TaskId}", receiptEdoTaskId);
			}
		}

		private async Task RollbackTransactionSafelyAsync(CancellationToken cancellationToken)
		{
			try
			{
				var tx = _unitOfWork.Session?.GetCurrentTransaction();
				if(tx != null && tx.IsActive)
				{
					await tx.RollbackAsync(cancellationToken);
				}
			}
			catch(Exception ex)
			{
				_logger.LogWarning(ex, "Ошибка при откате транзакции UnitOfWork");
			}
		}

		private ReceiptReprocessingEligibility CheckReprocessingEligibility(FiscalDocumentInfo fiscalInfo)
		{
			if(fiscalInfo?.FailureInfo == null)
			{
				return ReceiptReprocessingEligibility.Allowed();
			}

			var errorType = fiscalInfo.FailureInfo.Type ?? string.Empty;
			var message = fiscalInfo.FailureInfo.Message ?? string.Empty;
			var combinedText = $"{errorType} {message}".Trim();

			var criticalFiscalDriveErrors = _options.CurrentValue.CriticalFiscalDriveErrors;
			if(criticalFiscalDriveErrors != null)
			{
				foreach(var pattern in criticalFiscalDriveErrors)
				{
					if(!string.IsNullOrWhiteSpace(pattern)
						&& combinedText.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return ReceiptReprocessingEligibility.Blocked(
							$"Критический аппаратный сбой ФН ({pattern}): {combinedText}");
					}
				}
			}

			var criticalValidationErrors = _options.CurrentValue.CriticalValidationErrors;
			if(criticalValidationErrors != null)
			{
				foreach(var pattern in criticalValidationErrors)
				{
					if(!string.IsNullOrWhiteSpace(pattern)
						&& combinedText.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return ReceiptReprocessingEligibility.Blocked(
							$"Критическая ошибка валидации чека ({pattern}): {combinedText}");
					}
				}
			}

			_logger.LogInformation(
				"Неизвестный или некритичный тип ошибки фискализации кассы: Type='{ErrorType}', Message='{Message}'. Допускается к повторной отправке",
				errorType, message);

			return ReceiptReprocessingEligibility.Allowed();
		}

		private static readonly Regex _notFoundRegex =
			new Regex(@"\b(404|NotFound)\b|не найден", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex _transientHttpErrorRegex =
			new Regex(@"\b(429|500|502|503|504|TooManyRequests|ServiceUnavailable|BadGateway|GatewayTimeout)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex _nonTransientHttpErrorRegex =
			new Regex(@"\b(400|401|403|404|BadRequest|Unauthorized|Forbidden|NotFound)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static bool IsDocumentNotFound(string message)
		{
			if(string.IsNullOrWhiteSpace(message))
			{
				return false;
			}

			return _notFoundRegex.IsMatch(message);
		}

		private static bool IsNetworkOrInfrastructureException(Exception ex)
		{
			if(ex == null)
			{
				return false;
			}

			if(ex is TimeoutException
				|| ex is TaskCanceledException
				|| ex is System.Net.Sockets.SocketException
				|| ex is System.IO.IOException)
			{
				return true;
			}

			if(ex is HttpRequestException httpEx)
			{
				// Если ошибка содержит явный 4xx статус (400, 401, 403, 404), это не временный сетевой сбой
				if(_nonTransientHttpErrorRegex.IsMatch(httpEx.Message))
				{
					return false;
				}

				return true;
			}

			if(ex.InnerException != null && IsNetworkOrInfrastructureException(ex.InnerException))
			{
				return true;
			}

			if(!string.IsNullOrEmpty(ex.Message) && _transientHttpErrorRegex.IsMatch(ex.Message))
			{
				return true;
			}

			return false;
		}
	}
}
