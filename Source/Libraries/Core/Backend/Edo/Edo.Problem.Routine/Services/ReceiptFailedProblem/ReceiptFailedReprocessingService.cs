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
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using TrueMark.Contracts.Responses;
using TrueMark.Library;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Rules.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Settings.Edo;
using EdoFiscalStatus = Vodovoz.Core.Domain.Edo.FiscalDocumentStatus;
using ModulFiscalStatus = ModulKassa.DTO.FiscalDocumentStatus;

namespace Edo.Problem.Routine.Services.ReceiptFailedProblem
{
	/// <summary>
	/// Сервис мониторинга, проверки и автоматической переотправки чеков с ошибкой фискализации
	/// </summary>
	public class ReceiptFailedReprocessingService : IReceiptFailedReprocessingService
	{
		private readonly ILogger<ReceiptFailedReprocessingService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOptionsMonitor<ReceiptFailedReprocessingOptions> _options;
		private readonly IEdoRepository _edoRepository;
		private readonly CashboxClientProvider _cashboxClientProvider;
		private readonly FiscalDocumentFactory _fiscalDocumentFactory;
		private readonly ReceiptTrueMarkCodesPool _trueMarkCodesPool;
		private readonly ITrueMarkCodesPoolCodeProvider _trueMarkCodesPoolCodeProvider;
		private readonly Tag1260Checker _tag1260Checker;
		private readonly IEdoReceiptSettings _edoReceiptSettings;
		private readonly CanProcessOrSendEdoByClosingAccountingDate _canProcessOrSendEdo;
		private readonly IPublishEndpoint _publishEndpoint;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;

		/// <summary>
		/// Конструктор сервиса переобработки чеков с ошибками фискализации
		/// </summary>
		public ReceiptFailedReprocessingService(
			ILogger<ReceiptFailedReprocessingService> logger,
			IUnitOfWork unitOfWork,
			IOptionsMonitor<ReceiptFailedReprocessingOptions> options,
			IEdoRepository edoRepository,
			CashboxClientProvider cashboxClientProvider,
			FiscalDocumentFactory fiscalDocumentFactory,
			ReceiptTrueMarkCodesPool trueMarkCodesPool,
			ITrueMarkCodesPoolCodeProvider trueMarkCodesPoolCodeProvider,
			Tag1260Checker tag1260Checker,
			IEdoReceiptSettings edoReceiptSettings,
			CanProcessOrSendEdoByClosingAccountingDate canProcessOrSendEdo,
			IPublishEndpoint publishEndpoint,
			EdoProblemRegistrar edoProblemRegistrar = null)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
			_fiscalDocumentFactory = fiscalDocumentFactory ?? throw new ArgumentNullException(nameof(fiscalDocumentFactory));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_trueMarkCodesPoolCodeProvider = trueMarkCodesPoolCodeProvider ?? throw new ArgumentNullException(nameof(trueMarkCodesPoolCodeProvider));
			_tag1260Checker = tag1260Checker ?? throw new ArgumentNullException(nameof(tag1260Checker));
			_edoReceiptSettings = edoReceiptSettings ?? throw new ArgumentNullException(nameof(edoReceiptSettings));
			_canProcessOrSendEdo = canProcessOrSendEdo ?? throw new ArgumentNullException(nameof(canProcessOrSendEdo));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
			_edoProblemRegistrar = edoProblemRegistrar;
		}

		private int LookbackDays => _options.CurrentValue.LookbackDays > 0 ? _options.CurrentValue.LookbackDays : 7;
		private int FailedOlderThanHours => _options.CurrentValue.FailedOlderThanHours >= 0 ? _options.CurrentValue.FailedOlderThanHours : 24;

		/// <inheritdoc/>
		public async Task<ReprocessingExecutionResult> ProcessFailedDocumentsAsync(CancellationToken cancellationToken)
		{
			var executionResult = new ReprocessingExecutionResult();

			_logger.LogInformation(
				"Старт обработки чеков со статусом Failed за последние {LookbackDays} дн. (ошибка старше {FailedHours} ч.)",
				LookbackDays,
				FailedOlderThanHours);

			var failedOlderThan = DateTime.Now.AddHours(-FailedOlderThanHours);
			var lookbackFrom = DateTime.Today.AddDays(-LookbackDays);

			var failedDocuments = await _edoRepository.GetFailedFiscalDocumentsForReprocessing(
				_unitOfWork,
				statusChangedBefore: failedOlderThan,
				statusChangedNotBefore: lookbackFrom,
				cancellationToken: cancellationToken);

			executionResult.TotalFound = failedDocuments.Count;

			_logger.LogInformation("Найдено {Count} фискальных документов для проверки и переотправки", failedDocuments.Count);

			foreach(var edoFiscalDocument in failedDocuments)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var docId = edoFiscalDocument.Id;

				try
				{
					if(edoFiscalDocument.Status != EdoFiscalStatus.Failed)
					{
						_logger.LogInformation("Документ {DocId} уже не в статусе Failed (текущий: {Status}), пропускаем",
							docId, edoFiscalDocument.Status);
						executionResult.SuccessCount++;
						continue;
					}

					var receiptEdoTask = edoFiscalDocument.ReceiptEdoTask;
					if(receiptEdoTask == null)
					{
						_logger.LogWarning("Для фискального документа {DocId} не найдена связанная задача ЭДО", docId);
						executionResult.ErrorCount++;
						continue;
					}

					if(!receiptEdoTask.CashboxId.HasValue)
					{
						_logger.LogWarning("У задачи ЭДО {TaskId} для документа {DocId} не указана касса (CashboxId)",
							receiptEdoTask.Id, docId);
						executionResult.ErrorCount++;
						continue;
					}

					var documentGuid = edoFiscalDocument.DocumentGuid.ToString();

					_logger.LogInformation(
						"Проверка документа {DocId} (Guid: {Guid}, Касса: {CashboxId}) в МодульКассе",
						docId, documentGuid, receiptEdoTask.CashboxId.Value);

					var cashboxClient = await _cashboxClientProvider.GetCashboxAsync(receiptEdoTask.CashboxId.Value, cancellationToken);
					var modulCheckResult = await cashboxClient.CheckFiscalDocument(documentGuid, cancellationToken);

					if(modulCheckResult.SendStatus == SendStatus.Success
						&& modulCheckResult.FiscalDocumentInfo != null
						&& modulCheckResult.FiscalDocumentInfo.Status == ModulFiscalStatus.Completed)
					{
						_logger.LogInformation(
							"Документ {DocId} фактически фискализирован в МодульКассе со статусом COMPLETED. Обновляем статус в БД",
							docId);

						if(modulCheckResult.FiscalDocumentInfo.FiscalInfo != null)
						{
							ReceiptFiscalDocumentUpdater.UpdateFiscalDocument(edoFiscalDocument, modulCheckResult.FiscalDocumentInfo);
						}
						else
						{
							edoFiscalDocument.Status = EdoFiscalStatus.Completed;
							edoFiscalDocument.Stage = FiscalDocumentStage.Completed;
							edoFiscalDocument.StatusChangeTime = DateTime.Now;
						}

						edoFiscalDocument.FailureMessage = null;
						await _unitOfWork.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
						await CompleteReceiptAsync(receiptEdoTask, cancellationToken);
						await _unitOfWork.CommitAsync(cancellationToken);

						_logger.LogInformation(
							"Документ {DocId} успешно переведен в Completed и отправлено событие ReceiptCompleteEvent",
							docId);

						executionResult.SuccessCount++;
						continue;
					}

					_logger.LogInformation(
						"Документ {DocId} не фискализирован в МодульКассе (статус: {ModulStatus}). Подготовка к повторной отправке",
						docId, modulCheckResult.FiscalDocumentInfo?.Status.ToString() ?? "NOT_FOUND");

					var eligibility = CheckReprocessingEligibility(modulCheckResult.FiscalDocumentInfo);
					if(!eligibility.IsEligible)
					{
						_logger.LogWarning(
							"Документ {DocId} не может быть повторно отправлен автоматически: {Reason}",
							docId, eligibility.Reason);

						await HandleFailureAsync(edoFiscalDocument, receiptEdoTask, eligibility.Reason, cancellationToken);
						executionResult.ErrorCount++;
						continue;
					}

					var orderDate = receiptEdoTask.FormalEdoRequest?.Order?.DeliveryDate ?? edoFiscalDocument.CheckoutTime.Date;
					if(!_canProcessOrSendEdo.Check(orderDate))
					{
						_logger.LogWarning(
							"Документ {DocId} относится к закрытому периоду (дата: {OrderDate:dd.MM.yyyy}). Повторная отправка отменена",
							docId, orderDate);
						executionResult.SkippedClosedPeriodCount++;
						continue;
					}

					var isOlderThan24Hours = (DateTime.Now - edoFiscalDocument.CheckoutTime).TotalHours >= 24;
					if(isOlderThan24Hours)
					{
						_logger.LogInformation(
							"С момента оплаты документа {DocId} прошло более 24 часов ({Hours:F1} ч.). Формируется чек коррекции (SaleCorrection)",
							docId, (DateTime.Now - edoFiscalDocument.CheckoutTime).TotalHours);
						edoFiscalDocument.DocumentType = FiscalDocumentType.SaleCorrection;
					}
					else
					{
						edoFiscalDocument.DocumentType = FiscalDocumentType.Sale;
					}

	
					try
					{
						var codesReplaced = await CheckAndReplaceTrueMarkCodesAsync(edoFiscalDocument, receiptEdoTask, cancellationToken);
						if(codesReplaced > 0)
						{
							executionResult.ReplacedCodesCount += codesReplaced;
						}
					}
					catch(EdoCodePoolMissingCodeException ex)
					{
						_logger.LogWarning(ex, "Для документа {DocId} не удалось подобрать код из пула: {Message}", docId, ex.Message);
						await HandleFailureAsync(edoFiscalDocument, receiptEdoTask, ex.Message, cancellationToken);
						executionResult.ErrorCount++;
						continue;
					}
					catch(InvalidOperationException ex)
					{
						_logger.LogWarning(ex, "Ошибка подготовки кодов/реквизитов для документа {DocId}: {Message}", docId, ex.Message);
						await HandleFailureAsync(edoFiscalDocument, receiptEdoTask, ex.Message, cancellationToken);
						executionResult.ErrorCount++;
						continue;
					}

					var fiscalDocument = _fiscalDocumentFactory.CreateFiscalDocument(edoFiscalDocument);

					FiscalizationResult sendResult;

					if(_options.CurrentValue.DryRun)
					{
						_logger.LogWarning(
							"[DRY-RUN ЗАГЛУШКА] Реальная отправка документа {DocId} в МодульКассу отключена настройкой DryRun=true. Имитация успешной постановки в очередь",
							docId);

						sendResult = new FiscalizationResult
						{
							SendStatus = SendStatus.Success,
							FiscalDocumentInfo = new FiscalDocumentInfo
							{
								Status = ModulFiscalStatus.Queued,
								FnState = "DRY_RUN_MOCK"
							}
						};
					}
					else
					{
						_logger.LogInformation("Повторная отправка фискального документа {DocId} в МодульКассу", docId);
						sendResult = await cashboxClient.SendFiscalDocument(fiscalDocument, cancellationToken);
					}

					if(sendResult.SendStatus == SendStatus.Success)
					{
						if(sendResult.FiscalDocumentInfo?.FiscalInfo != null)
						{
							ReceiptFiscalDocumentUpdater.UpdateFiscalDocument(edoFiscalDocument, sendResult.FiscalDocumentInfo);
						}
						else
						{
							edoFiscalDocument.Stage = FiscalDocumentStage.Sent;
							edoFiscalDocument.Status = sendResult.FiscalDocumentInfo != null
								? ReceiptConverters.ConvertFiscalDocumentStatus(sendResult.FiscalDocumentInfo.Status)
								: EdoFiscalStatus.Queued;
							edoFiscalDocument.StatusChangeTime = DateTime.Now;
						}

						edoFiscalDocument.FailureMessage = null;

						if(edoFiscalDocument.Status == EdoFiscalStatus.Completed)
						{
							await CompleteReceiptAsync(receiptEdoTask, cancellationToken);
						}
						else
						{
							receiptEdoTask.ReceiptStatus = EdoReceiptStatus.Sending;
							await _unitOfWork.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
						}

						await _unitOfWork.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
						await _unitOfWork.CommitAsync(cancellationToken);

						_logger.LogInformation("Документ {DocId} успешно отправлен в МодульКассу", docId);
						executionResult.SuccessCount++;
					}
					else
					{
						_logger.LogWarning(
							"МодульКасса отклонила повторную отправку документа {DocId}. Ошибка: {Error}",
							docId, sendResult.ErrorMessage);

						var failureReason = sendResult.FiscalDocumentInfo?.FailureInfo?.Message
							?? sendResult.FiscalDocumentInfo?.Message
							?? sendResult.ErrorMessage
							?? "Ошибка при повторной отправке в МодульКассу";

						await HandleFailureAsync(edoFiscalDocument, receiptEdoTask, failureReason, cancellationToken);
						executionResult.ErrorCount++;
					}
				}
				catch(OperationCanceledException)
				{
					throw;
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Непредвиденная ошибка при обработке документа {DocId}", docId);
					executionResult.ErrorCount++;
				}
			}

			executionResult.Message =
				$"Обработка завершена. Всего: {executionResult.TotalFound}, " +
				$"успешно: {executionResult.SuccessCount}, " +
				$"заменено кодов: {executionResult.ReplacedCodesCount}, " +
				$"пропущено (закрытый период): {executionResult.SkippedClosedPeriodCount}, " +
				$"ошибок: {executionResult.ErrorCount}.";

			_logger.LogInformation(executionResult.Message);
			return executionResult;
		}

		private async Task CompleteReceiptAsync(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			receiptEdoTask.ReceiptStatus = EdoReceiptStatus.Completed;
			receiptEdoTask.Status = EdoTaskStatus.Completed;

			if(receiptEdoTask.Problems != null)
			{
				foreach(var problem in receiptEdoTask.Problems.Where(x => x.State == TaskProblemState.Active))
				{
					problem.State = TaskProblemState.Solved;
					await _unitOfWork.SaveAsync(problem, cancellationToken: cancellationToken);
				}
			}

			if(_edoProblemRegistrar != null)
			{
				_edoProblemRegistrar.SolveCustomProblem<ReceiptFiscalizationFailed>(receiptEdoTask);
				_edoProblemRegistrar.SolveCustomProblem<ReceiptSendingFailed>(receiptEdoTask);
			}

			await _publishEndpoint.Publish(
				new ReceiptCompleteEvent { ReceiptEdoTaskId = receiptEdoTask.Id },
				cancellationToken);
		}

		/// <summary>
		/// Фиксирует причину ошибки в фискальном документе и регистрирует проблему в задаче ЭДО
		/// </summary>
		/// <param name="edoFiscalDocument">Фискальный документ с ошибкой</param>
		/// <param name="receiptEdoTask">Связанная задача ЭДО</param>
		/// <param name="failureReason">Текст причины ошибки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		private async Task HandleFailureAsync(
			EdoFiscalDocument edoFiscalDocument,
			ReceiptEdoTask receiptEdoTask,
			string failureReason,
			CancellationToken cancellationToken)
		{
			edoFiscalDocument.FailureMessage = failureReason;
			edoFiscalDocument.StatusChangeTime = DateTime.Now;

			await _unitOfWork.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
			await _unitOfWork.CommitAsync(cancellationToken);

			if(_edoProblemRegistrar != null && receiptEdoTask != null)
			{
				await RegisterProblemAsync(receiptEdoTask.Id, failureReason, cancellationToken);
			}
		}

		/// <summary>
		/// Регистрирует проблему фискализации чека в ЭДО для ручного разбора или последующего регламентного решения
		/// </summary>
		/// <param name="receiptEdoTaskId">Идентификатор задачи ЭДО чека</param>
		/// <param name="failureMessage">Текст ошибки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		private async Task RegisterProblemAsync(
			int receiptEdoTaskId,
			string failureMessage,
			CancellationToken cancellationToken)
		{
			if(_edoProblemRegistrar == null || receiptEdoTaskId <= 0)
			{
				return;
			}

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
				_logger.LogWarning(ex, "Не удалось зарегистрировать проблему фискализации для задачи {TaskId}", receiptEdoTaskId);
			}
		}

		/// <summary>
		/// Проверяет допустимость автоматической переотправки чека по данным МодульКассы.
		/// Блокирует переотправку только при фатальных ошибках (переполнение памяти ФН, криптографическая ошибка ФН,
		/// синтаксическая ошибка реквизитов заказа), требующих ручного вмешательства.
		/// </summary>
		/// <param name="fiscalInfo">Информация о фискальном документе из МодульКассы</param>
		/// <returns>Результат проверки допустимости переотправки</returns>
		private static ReceiptReprocessingEligibility CheckReprocessingEligibility(FiscalDocumentInfo fiscalInfo)
		{
			if(fiscalInfo?.FailureInfo == null)
			{
				return ReceiptReprocessingEligibility.Allowed();
			}

			var errorType = fiscalInfo.FailureInfo.Type;
			if(string.IsNullOrWhiteSpace(errorType))
			{
				return ReceiptReprocessingEligibility.Allowed();
			}

			if(errorType.IndexOf("FnMemoryFull", StringComparison.OrdinalIgnoreCase) >= 0
				|| errorType.IndexOf("FnCrypt", StringComparison.OrdinalIgnoreCase) >= 0
				|| errorType.IndexOf("ResourceExhausted", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return ReceiptReprocessingEligibility.Blocked(
					$"Критический сбой фискального накопителя: {errorType} ({fiscalInfo.FailureInfo.Message})");
			}

			if(errorType.IndexOf("ValidationError", StringComparison.OrdinalIgnoreCase) >= 0
				|| errorType.IndexOf("InvalidPaymentDetails", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return ReceiptReprocessingEligibility.Blocked(
					$"Ошибка реквизитов документа: {errorType} ({fiscalInfo.FailureInfo.Message})");
			}

			return ReceiptReprocessingEligibility.Allowed();
		}

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

			var regulatoryDocument = GetRegulatoryDocument();
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

				// Проверка разрешительного режима, валидности и остаточного срока годности
				var canSell = codeInfo != null
					&& codeInfo.ErrorCode == 0
					&& codeInfo.Found
					&& codeInfo.Valid
					&& codeInfo.Verified
					&& codeInfo.ExpireDate > DateTime.Now
					&& codeInfo.Realizable
					&& codeInfo.Utilised
					&& !codeInfo.IsBlocked
					&& !codeInfo.Sold;

				if(canSell)
				{
					_logger.LogInformation(
						"Код маркировки для позиции {PosName} прошел проверку тега 1260 и разрешительного режима. Обновляем данные реквизита",
						pos.Name);

					if(isProductCode && productCode.ResultCode == null)
					{
						productCode.ResultCode = productCode.SourceCode;
						await _unitOfWork.SaveAsync(productCode, cancellationToken: cancellationToken);
					}

					pos.IndustryRequisiteData = $"UUID={checkResponse.ReqId}&Time={checkResponse.ReqTimestamp}";
					pos.RegulatoryDocument = regulatoryDocument;
					await _unitOfWork.SaveAsync(pos, cancellationToken: cancellationToken);
				}
				else
				{
					if(!isProductCode)
					{
						throw new InvalidOperationException(
							$"Групповой код маркировки для позиции {pos.Name} не прошел проверку разрешительного режима (ErrorCode={codeInfo?.ErrorCode}), автозамена групповых кодов не поддерживается");
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

			foreach(var item in positionsNeedingNewCodes)
			{
				var gtin = item.ProductCode.ResultCode?.Gtin ?? item.ProductCode.SourceCode?.Gtin;

				_logger.LogInformation(
					"Код маркировки для позиции {PosName} (GTIN: {GTIN}) требует замены. Подбор из пула кодов для собственных нужд",
					item.Position.Name, gtin);

				var gtinEntity = await _edoRepository.GetGtinByGtinNumberAsync(gtin, cancellationToken)
					?? new GtinEntity { GtinNumber = gtin };

				var newCode = await _trueMarkCodesPoolCodeProvider.TakeValidCodeAsync(
					_trueMarkCodesPool,
					gtinEntity,
					organizationInn,
					cancellationToken,
					addingTimeNotAfter: docCreationTime);

				if(newCode == null)
				{
					throw new InvalidOperationException(
						$"В пуле кодов не найден подходящий код для GTIN {gtin} с датой добавления до {docCreationTime:yyyy-MM-dd HH:mm:ss}");
				}

				_logger.LogInformation(
					"Код маркировки для позиции {PosId} заменен на {NewCodeId}",
					item.Position.Id, newCode.Id);

				item.ProductCode.ResultCode = newCode;
				item.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;
				await _unitOfWork.SaveAsync(item.ProductCode, cancellationToken: cancellationToken);

				item.NewCode = newCode;
			}

			// Пакетный запрос реквизитов тега 1260 для всех замененных кодов
			var newCodesToCheck = positionsNeedingNewCodes
				.Select(x => x.NewCode.FormatForCheck1260)
				.Distinct()
				.ToList();

			var newCheckResponse = await _tag1260Checker.CheckCodesForTag1260Async(
				newCodesToCheck,
				cashBoxToken.Value,
				cancellationToken);

			if(newCheckResponse == null || newCheckResponse.Code != 0)
			{
				throw new InvalidOperationException(
					$"Не удалось получить реквизиты тега 1260 для новых кодов: Code={newCheckResponse?.Code}, Description={newCheckResponse?.Description}");
			}

			foreach(var item in positionsNeedingNewCodes)
			{
				item.Position.IndustryRequisiteData = $"UUID={newCheckResponse.ReqId}&Time={newCheckResponse.ReqTimestamp}";
				item.Position.RegulatoryDocument = regulatoryDocument;
				await _unitOfWork.SaveAsync(item.Position, cancellationToken: cancellationToken);
			}

			return positionsNeedingNewCodes.Count;
		}

		private FiscalIndustryRequisiteRegulatoryDocument GetRegulatoryDocument()
		{
			var regDocId = _edoReceiptSettings.IndustryRequisiteRegulatoryDocumentId;
			if(regDocId <= 0)
			{
				throw new InvalidOperationException("Не указан Id регуляторного документа в настройках IndustryRequisiteRegulatoryDocumentId");
			}

			var regDoc = _unitOfWork.GetById<FiscalIndustryRequisiteRegulatoryDocument>(regDocId);
			if(regDoc == null)
			{
				throw new InvalidOperationException($"Не найден регуляторный документ (Id: {regDocId}) для формирования реквизита 1260");
			}

			return regDoc;
		}

		private static string GetPositionCis(FiscalInventPosition pos)
		{
			return pos.EdoTaskItem?.ProductCode != null
				? (pos.EdoTaskItem.ProductCode.ResultCode ?? pos.EdoTaskItem.ProductCode.SourceCode)?.FormatForCheck1260
				: pos.GroupCode?.FormatForCheck1260;
		}

		/// <summary>
		/// Результат проверки допустимости автоматической повторной фискализации чека
		/// </summary>
		private class ReceiptReprocessingEligibility
		{
			/// <summary>
			/// Признак, разрешена ли автоматическая повторная отправка документа
			/// </summary>
			public bool IsEligible { get; }

			/// <summary>
			/// Причина запрета автоматической повторной отправки (если IsEligible == false)
			/// </summary>
			public string Reason { get; }

			private ReceiptReprocessingEligibility(bool isEligible, string reason = null)
			{
				IsEligible = isEligible;
				Reason = reason;
			}

			/// <summary>
			/// Повторная отправка разрешена
			/// </summary>
			public static ReceiptReprocessingEligibility Allowed() =>
				new ReceiptReprocessingEligibility(true);

			/// <summary>
			/// Повторная отправка заблокирована с указанием причины
			/// </summary>
			/// <param name="reason">Причина блокировки</param>
			public static ReceiptReprocessingEligibility Blocked(string reason) =>
				new ReceiptReprocessingEligibility(false, reason);
		}

		/// <summary>
		/// Контекст позиции чека, требующей подбора и замены кода маркировки
		/// </summary>
		private class PositionCodeReplacementContext
		{
			/// <summary>
			/// Позиция фискального чека
			/// </summary>
			public FiscalInventPosition Position { get; set; }

			/// <summary>
			/// Код товара в задаче ЭДО
			/// </summary>
			public TrueMarkProductCode ProductCode { get; set; }

			/// <summary>
			/// Новый подобранный из пула код маркировки
			/// </summary>
			public TrueMarkWaterIdentificationCode NewCode { get; set; }
		}
	}
}
