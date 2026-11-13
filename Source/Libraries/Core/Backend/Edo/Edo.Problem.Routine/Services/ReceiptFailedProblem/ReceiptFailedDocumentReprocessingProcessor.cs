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
using TrueMark.Library;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Settings.Edo;
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
		private readonly Tag1260Checker _tag1260Checker;
		private readonly IEdoReceiptSettings _edoReceiptSettings;
		private readonly IPublishEndpoint _publishEndpoint;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private ReceiptTrueMarkCodesPool _trueMarkCodesPool;
		private FiscalIndustryRequisiteRegulatoryDocument _regulatoryDocument;

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
			Tag1260Checker tag1260Checker,
			IEdoReceiptSettings edoReceiptSettings,
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
			_tag1260Checker = tag1260Checker ?? throw new ArgumentNullException(nameof(tag1260Checker));
			_edoReceiptSettings = edoReceiptSettings ?? throw new ArgumentNullException(nameof(edoReceiptSettings));
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
				await IncrementReprocessingRetryCountAsync(receiptEdoTask.Id, cancellationToken);
				return FiscalDocumentReprocessingResult.Error();
			}

			// Проверка количества предыдущих попыток переобработки
			var problem = receiptEdoTask.Problems?.FirstOrDefault(p =>
				p.State == TaskProblemState.Active
				&& p.SourceName != null
				&& p.SourceName.IndexOf(nameof(ReceiptFiscalizationFailed), StringComparison.OrdinalIgnoreCase) >= 0);
			var routineState = problem?.RoutineStates?.FirstOrDefault();

			var maxReprocessingAttempts = _options.CurrentValue.MaxReprocessingAttempts;
			if(maxReprocessingAttempts > 0 && routineState != null && routineState.RetryCount >= maxReprocessingAttempts)
			{
				var limitMessage = $"Исчерпан лимит автоматических попыток повторной отправки ({routineState.RetryCount} из {maxReprocessingAttempts}). Требуется ручной разбор";
				_logger.LogWarning(
					"Документ {DocId} задачи {TaskId} исчерпал лимит автоматических попыток повторной отправки ({RetryCount} из {MaxAttempts}). Повторная отправка отменена",
					fiscalDocumentId, receiptEdoTask.Id, routineState.RetryCount, maxReprocessingAttempts);

				var newFailureMessage = string.IsNullOrWhiteSpace(edoFiscalDocument.FailureMessage)
					? limitMessage
					: (edoFiscalDocument.FailureMessage.Contains("Исчерпан лимит")
						? edoFiscalDocument.FailureMessage
						: $"{edoFiscalDocument.FailureMessage} | {limitMessage}");

				if(edoFiscalDocument.FailureMessage != newFailureMessage)
				{
					edoFiscalDocument.FailureMessage = newFailureMessage;
					await _unitOfWork.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
					await _unitOfWork.CommitAsync(cancellationToken);
				}

				return FiscalDocumentReprocessingResult.Skipped();
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
				await IncrementReprocessingRetryCountAsync(receiptEdoTask.Id, cancellationToken);
				return FiscalDocumentReprocessingResult.Error();
			}
			catch(Tag1260CdnUnavailableException ex)
			{
				await RollbackTransactionSafelyAsync(cancellationToken);
				_unitOfWork.Session.Clear();

				_logger.LogWarning(ex, "Сбой связи с CDN Честного Знака для документа {DocId}", fiscalDocumentId);
				await HandleFailureInCleanSessionAsync(
					fiscalDocumentId,
					receiptEdoTask.Id,
					$"Сбой связи с CDN Честного Знака: {ex.Message}",
					cancellationToken);
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
				await IncrementReprocessingRetryCountAsync(receiptEdoTask.Id, cancellationToken);
				return FiscalDocumentReprocessingResult.Error();
			}

			FiscalizationResult sendResult;
			var isDocumentNotFoundInCashbox = modulCheckResult.SendStatus == SendStatus.Error
				&& IsDocumentNotFound(modulCheckResult.ErrorMessage);

			var isPayloadChanged = codesReplaced > 0;

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

				await IncrementReprocessingRetryCountAsync(receiptEdoTask.Id, cancellationToken);
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

				await IncrementReprocessingRetryCountAsync(receiptEdoTask.Id, cancellationToken);
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

				await IncrementReprocessingRetryCountAsync(receiptEdoTask.Id, cancellationToken);
				await _unitOfWork.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
				await _unitOfWork.CommitAsync(cancellationToken);

				sendResult = await cashboxClient.RequeueFiscalDocument(guid.ToString(), cancellationToken);
			}

			if(sendResult.SendStatus == SendStatus.Success)
			{
				// Если касса вернула статус Failed
				if(sendResult.FiscalDocumentInfo?.Status == ModulFiscalStatus.Failed)
				{
					var failureMsg = sendResult.FiscalDocumentInfo.FailureInfo?.Message
						?? sendResult.ErrorMessage
						?? "Документ отклонен сервером фискализации (статус Failed)";

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
			var cashBoxToken = seller?.CashBoxTokenFromTrueMark;

			if(!cashBoxToken.HasValue)
			{
				throw new InvalidOperationException(
					$"У организации Id {seller?.Id} отсутствует токен ЧЗ (CashBoxTokenFromTrueMark) для проверки тега 1260 документа {edoFiscalDocument.Id}");
			}

			var docCreationTime = edoFiscalDocument.CreationTime;

			var codesToCheck = markedPositions
				.Select(GetPositionCis)
				.Where(c => !string.IsNullOrEmpty(c))
				.Distinct()
				.ToList();

			_logger.LogInformation(
				"Проверка {Count} уникальных кодов маркировки по тегу 1260 и разрешительному режиму для документа {DocId}",
				codesToCheck.Count, edoFiscalDocument.Id);

			var checkResponse = await _tag1260Checker.CheckCodesForTag1260Async(
				codesToCheck,
				cashBoxToken.Value,
				cancellationToken);

			if(checkResponse == null || checkResponse.Code != 0)
			{
				throw new InvalidOperationException(
					$"Честный Знак вернул ошибку при проверке тега 1260: Code={checkResponse?.Code}, Description={checkResponse?.Description}");
			}

			var positionsNeedingNewCodes = new List<PositionCodeReplacementContext>();

			foreach(var pos in markedPositions)
			{
				var isProductCode = pos.EdoTaskItem?.ProductCode != null;
				var productCode = pos.EdoTaskItem?.ProductCode;
				var cis = GetPositionCis(pos);

				if(string.IsNullOrEmpty(cis))
				{
					continue;
				}

				var codeInfo = checkResponse.Codes?.FirstOrDefault(c => c.Cis == cis);

				// Проверка разрешительного режима, валидности и остаточного срока годности (с учетом nullable/default даты)
				var canSell = CanSellByTag1260(codeInfo);

				if(canSell)
				{
					_logger.LogInformation(
						"Код маркировки для позиции {PosName} прошел проверку тега 1260 и разрешительного режима (валиден)",
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
							$"Групповой код маркировки для позиции \"{pos.Name}\" не прошел проверку разрешительного режима (ErrorCode={codeInfo?.ErrorCode}). Автозамена групповых кодов не поддерживается, требуется ручная обработка фин.отделом.");
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

			var regulatoryDocument = GetRegulatoryDocument();

			var reasonForLeaving = receiptEdoTask.FormalEdoRequest?.Order?.Client?.ReasonForLeaving;
			var canReplaceCodes = reasonForLeaving != ReasonForLeaving.Resale && reasonForLeaving != ReasonForLeaving.Tender;

			if(!canReplaceCodes)
			{
				throw new InvalidOperationException(
					$"Для причины выбытия \"{reasonForLeaving}\" автозамена кодов маркировки из пула запрещена. " +
					$"Позиции чека ({positionsNeedingNewCodes.Count} шт.) не прошли проверку разрешительного режима в Честном Знаке. " +
					"Требуется ручной разбор фин. отделом / складом.");
			}

			// Цикл подбора кодов из пула с валидацией по тегу 1260 и разрешительному режиму в ЧЗ (до 5 попыток)
			const int maxAttempts = 5;
			var positionsRemaining = new List<PositionCodeReplacementContext>(positionsNeedingNewCodes);

			for(var attempt = 1; attempt <= maxAttempts && positionsRemaining.Count > 0; attempt++)
			{
				// Подбираем код из пула для каждой позиции, еще не прошедшей проверку
				foreach(var item in positionsRemaining)
				{
					var gtin = item.ProductCode.ResultCode?.Gtin ?? item.ProductCode.SourceCode?.Gtin;

					_logger.LogInformation(
						"Подбор кода из пула для позиции \"{PosName}\" (GTIN: {GTIN}, попытка {Attempt} из {MaxAttempts})",
						item.Position.Name, gtin, attempt, maxAttempts);

					var gtinEntity = await _edoRepository.GetGtinByGtinNumberAsync(gtin, cancellationToken)
						?? new GtinEntity { GtinNumber = gtin };

					var newCode = await _trueMarkCodesPoolCodeProvider.TakeValidCodeAsync(
						CodesPool,
						gtinEntity,
						organizationInn,
						cancellationToken,
						addingTimeNotAfter: docCreationTime);

					item.NewCode = newCode;
				}

				// Пакетная проверка кандидатов из пула по тегу 1260 в Честном Знаке
				var candidatesToCheck = positionsRemaining
					.Select(x => x.NewCode.FormatForCheck1260)
					.Distinct()
					.ToList();

				var tag1260Result = await _tag1260Checker.CheckCodesForTag1260Async(
					candidatesToCheck,
					cashBoxToken.Value,
					cancellationToken);

				if(tag1260Result == null || tag1260Result.Code != 0)
				{
					throw new InvalidOperationException(
						$"Честный Знак вернул ошибку при проверке тега 1260 для кодов из пула: Code={tag1260Result?.Code}, Description={tag1260Result?.Description}");
				}

				var positionsToRetry = new List<PositionCodeReplacementContext>();

				foreach(var item in positionsRemaining)
				{
					var cis = item.NewCode.FormatForCheck1260;
					var codeInfo = tag1260Result.Codes?.FirstOrDefault(c => c.Cis == cis);

					if(CanSellByTag1260(codeInfo))
					{
						_logger.LogInformation(
							"Код из пула {NewCodeId} для позиции {PosName} успешно прошел проверку разрешительного режима (попытка {Attempt})",
							item.NewCode.Id, item.Position.Name, attempt);

						item.ProductCode.ResultCode = item.NewCode;
						item.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;
						await _unitOfWork.SaveAsync(item.ProductCode, cancellationToken: cancellationToken);

						item.Position.IndustryRequisiteData = $"UUID={tag1260Result.ReqId}&Time={NormalizeReqTimestamp(tag1260Result.ReqTimestamp)}";
						item.Position.RegulatoryDocument = regulatoryDocument;
						await _unitOfWork.SaveAsync(item.Position, cancellationToken: cancellationToken);
					}
					else
					{
						_logger.LogWarning(
							"Код из пула {NewCodeId} ({Cis}) для позиции \"{PosName}\" не прошел проверку разрешительного режима ЧЗ (ErrorCode: {ErrorCode}, Realizable: {Realizable}, Sold: {Sold}, Blocked: {Blocked}). Попытка {Attempt} из {MaxAttempts}",
							item.NewCode.Id, cis, item.Position.Name, codeInfo?.ErrorCode, codeInfo?.Realizable, codeInfo?.Sold, codeInfo?.IsBlocked, attempt, maxAttempts);

						positionsToRetry.Add(item);
					}
				}

				positionsRemaining = positionsToRetry;
			}

			if(positionsRemaining.Count > 0)
			{
				throw new EdoCodePoolMissingCodeException(
					$"Не удалось подобрать из пула коды, проходящие проверку тега 1260 в Честном Знаке за {maxAttempts} попыток (не прошли {positionsRemaining.Count} поз.)");
			}

			return positionsNeedingNewCodes.Count;
		}

		private FiscalIndustryRequisiteRegulatoryDocument GetRegulatoryDocument()
		{
			if(_regulatoryDocument != null)
			{
				return _regulatoryDocument;
			}

			var regDocId = _edoReceiptSettings.IndustryRequisiteRegulatoryDocumentId;
			if(regDocId <= 0)
			{
				throw new InvalidOperationException("Не указан Id регуляторного документа в настройках IndustryRequisiteRegulatoryDocumentId");
			}

			_regulatoryDocument = _unitOfWork.GetById<FiscalIndustryRequisiteRegulatoryDocument>(regDocId);
			if(_regulatoryDocument == null)
			{
				throw new InvalidOperationException($"Не найден регуляторный документ (Id: {regDocId}) для формирования реквизита 1260");
			}

			return _regulatoryDocument;
		}

		private static string GetPositionCis(FiscalInventPosition pos)
		{
			return pos.EdoTaskItem?.ProductCode != null
				? (pos.EdoTaskItem.ProductCode.ResultCode ?? pos.EdoTaskItem.ProductCode.SourceCode)?.FormatForCheck1260
				: pos.GroupCode?.FormatForCheck1260;
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
				await RegisterProblemAsync(receiptEdoTaskId, failureReason, cancellationToken);
			}
		}

		private async Task RegisterProblemAsync(
			int receiptEdoTaskId,
			string failureMessage,
			CancellationToken cancellationToken)
		{
			try
			{
				await _edoProblemRegistrar.RegisterCustomProblem<ReceiptFiscalizationFailed>(
					receiptEdoTaskId,
					cancellationToken,
					customMessage: failureMessage,
					disposeTaskUow: false);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Не удалось зарегистрировать проблему фискализации для задачи {TaskId}", receiptEdoTaskId);
			}
		}

		/// <summary>
		/// Увеличивает счетчик попыток повторной отправки в RoutineState активной проблемы задачи (если она существует).
		/// Если активной проблемы еще нет, ложная проблема операторам при штатной переотправке не создается.
		/// </summary>
		private async Task IncrementReprocessingRetryCountAsync(int receiptEdoTaskId, CancellationToken cancellationToken)
		{
			try
			{
				using(var uow = _uowFactory.CreateWithoutRoot())
				{
					var task = uow.GetById<ReceiptEdoTask>(receiptEdoTaskId);
					var problem = task?.Problems?.FirstOrDefault(p =>
						p.State == TaskProblemState.Active
						&& p.SourceName != null
						&& p.SourceName.IndexOf(nameof(ReceiptFiscalizationFailed), StringComparison.OrdinalIgnoreCase) >= 0);

					if(problem == null)
					{
						return;
					}

					var routineState = problem.RoutineStates?.FirstOrDefault();
					if(routineState == null)
					{
						routineState = new EdoTaskProblemRoutineState
						{
							Problem = problem,
							RetryCount = 1,
							LastRetryTime = DateTime.Now
						};
						if(problem.RoutineStates == null)
						{
							problem.RoutineStates = new List<EdoTaskProblemRoutineState>();
						}
						problem.RoutineStates.Add(routineState);
						await uow.SaveAsync(routineState, cancellationToken: cancellationToken);
					}
					else
					{
						routineState.RetryCount++;
						routineState.LastRetryTime = DateTime.Now;
						await uow.SaveAsync(routineState, cancellationToken: cancellationToken);
					}

					await uow.SaveAsync(problem, cancellationToken: cancellationToken);
					await uow.CommitAsync(cancellationToken);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Не удалось обновить счетчик повторной фискализации для задачи {TaskId}", receiptEdoTaskId);
			}
		}

		private async Task RollbackTransactionSafelyAsync(CancellationToken cancellationToken)
		{
			_regulatoryDocument = null;
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

		private static long NormalizeReqTimestamp(long timestamp)
		{
			// Если timestamp передан в миллисекундах (13 цифр, > 100_000_000_000), переводим в секунды (10 цифр)
			if(timestamp > 100_000_000_000L)
			{
				return timestamp / 1000;
			}

			return timestamp;
		}

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

			if(ex is Tag1260ApiException apiEx)
			{
				return apiEx.IsTransient;
			}

			if(ex is Tag1260CdnUnavailableException
				|| ex is TimeoutException
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

		/// <summary>
		/// Проверяет, допускается ли код маркировки к розничной продаже по разрешительному режиму (тег 1260)
		/// </summary>
		private bool CanSellByTag1260(TrueMark.Contracts.CodeCheckInfo codeInfo)
		{
			var isNotExpired = codeInfo != null && (codeInfo.ExpireDate == default || codeInfo.ExpireDate > DateTime.Now);
			return codeInfo != null
				&& codeInfo.ErrorCode == 0
				&& codeInfo.Found
				&& codeInfo.Valid
				&& codeInfo.Verified
				&& isNotExpired
				&& codeInfo.Realizable
				&& codeInfo.Utilised
				&& !codeInfo.IsBlocked
				&& !codeInfo.Sold;
		}
	}
}
