﻿using Core.Infrastructure;
using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Exception;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using TrueMark.Library;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Client;
using Vodovoz.Settings.Edo;

namespace Edo.Receipt.Dispatcher
{
	public class ForOwnNeedsReceiptEdoTaskHandler
	{
		private readonly ILogger<ForOwnNeedsReceiptEdoTaskHandler> _logger;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly TransferRequestCreator _transferRequestCreator;
		private readonly IEdoRepository _edoRepository;
		private readonly IEdoReceiptSettings _edoReceiptSettings;
		private readonly TrueMarkTaskCodesValidator _localCodesValidator;
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly Tag1260Checker _tag1260Checker;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly IGenericRepository<TrueMarkProductCode> _productCodeRepository;
		private readonly IBus _messageBus;
		private readonly IUnitOfWork _uow;
		private readonly EdoTaskValidator _edoTaskValidator;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly int _maxCodesInReceipt;

		private int _prepareReceiptAttempts = 3;

		public ForOwnNeedsReceiptEdoTaskHandler(
			ILogger<ForOwnNeedsReceiptEdoTaskHandler> logger,
			IUnitOfWork uow,
			EdoTaskValidator edoTaskValidator,
			EdoProblemRegistrar edoProblemRegistrar,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			TransferRequestCreator transferRequestCreator,
			IEdoRepository edoRepository,
			IEdoReceiptSettings edoReceiptSettings,
			TrueMarkTaskCodesValidator localCodesValidator,
			TrueMarkCodesPool trueMarkCodesPool,
			Tag1260Checker tag1260Checker,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			IGenericRepository<TrueMarkProductCode> productCodeRepository,
			IBus messageBus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_edoTaskValidator = edoTaskValidator ?? throw new ArgumentNullException(nameof(edoTaskValidator));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_edoTaskTrueMarkCodeCheckerFactory = edoTaskTrueMarkCodeCheckerFactory ?? throw new ArgumentNullException(nameof(edoTaskTrueMarkCodeCheckerFactory));
			_transferRequestCreator = transferRequestCreator ?? throw new ArgumentNullException(nameof(transferRequestCreator));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_edoReceiptSettings = edoReceiptSettings ?? throw new ArgumentNullException(nameof(edoReceiptSettings));
			_localCodesValidator = localCodesValidator ?? throw new ArgumentNullException(nameof(localCodesValidator));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_tag1260Checker = tag1260Checker ?? throw new ArgumentNullException(nameof(tag1260Checker));
			_productCodeRepository = productCodeRepository ?? throw new ArgumentNullException(nameof(productCodeRepository));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));

			_maxCodesInReceipt = _edoReceiptSettings.MaxCodesInReceiptCount;
		}

		public async Task HandleForOwnNeedsReceipt(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			var order = receiptEdoTask.OrderEdoRequest.Order;
			if(order.Client.ReasonForLeaving == ReasonForLeaving.Resale)
			{
				throw new InvalidOperationException($"Попытка обработать чек с причиной выбытия " +
					$"{order.Client.ReasonForLeaving} обработчиком для {ReasonForLeaving.ForOwnNeeds}.");
			}

			// предзагрузка для ускорения
			await _uow.Session.QueryOver<TrueMarkProductCode>()
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Where(x => x.CustomerEdoRequest.Id == receiptEdoTask.OrderEdoRequest.Id)
				.ListAsync();


			var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(receiptEdoTask);
			var isValid = await _edoTaskValidator.Validate(receiptEdoTask, cancellationToken, trueMarkCodesChecker);
			if(!isValid)
			{
				return;
			}

			// принудительная отправка чека
			var hasManualSend = receiptEdoTask.OrderEdoRequest.Source == CustomerEdoRequestSource.Manual;
			if(hasManualSend)
			{
				await PrepareReceipt(receiptEdoTask, trueMarkCodesChecker, cancellationToken);
				return;
			}

			// всегда отправлять чек клиенту
			var hasAlwaysSend = receiptEdoTask.OrderEdoRequest.Order.Client.AlwaysSendReceipts;
			if(hasAlwaysSend)
			{
				await PrepareReceipt(receiptEdoTask, trueMarkCodesChecker, cancellationToken);
				return;
			}

			// проверка на наличие чека на сумму за сегодня
			var hasReceiptOnSumToday = await HasReceiptOnSumToday(receiptEdoTask, cancellationToken);
			if(!hasReceiptOnSumToday)
			{
				await PrepareReceipt(receiptEdoTask, trueMarkCodesChecker, cancellationToken);
				return;
			}

			// сохранение кодов в пул
			// смена статуса задачи на завершен
			// сохранение задачи
			await SaveCodesToPool(receiptEdoTask, cancellationToken);
			receiptEdoTask.Status = EdoTaskStatus.Completed;
			receiptEdoTask.ReceiptStatus = EdoReceiptStatus.SavedToPool;
			receiptEdoTask.EndTime = DateTime.Now;
			await _uow.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
		}

		public async Task HandleTransferComplete(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(receiptEdoTask);

			var taskValidationResult = await _localCodesValidator.ValidateAsync(
					receiptEdoTask,
					trueMarkCodesChecker,
					cancellationToken
				);

			if(!taskValidationResult.ReadyToSell)
			{
				await PrepareReceipt(receiptEdoTask, trueMarkCodesChecker, cancellationToken);
				return;
			}

			// итоговая валидация и получение разрешительного режима
			var industryRequisitePrepareResult = await PrepareIndustryRequisite(receiptEdoTask, cancellationToken);
			switch(industryRequisitePrepareResult)
			{
				case IndustryRequisitePrepareResult.Succeeded:
					// Ничего не делаем, продолжаем
					break;
				case IndustryRequisitePrepareResult.NeedToChange:
					await PrepareReceipt(receiptEdoTask, trueMarkCodesChecker, cancellationToken);
					return;
				case IndustryRequisitePrepareResult.Problem:
					return;
				default:
					throw new NotSupportedException($"Неизвестный результат подготовки разрешительного " +
						$"режима: {industryRequisitePrepareResult}");
			}

			// перевод в отправку
			receiptEdoTask.Status = EdoTaskStatus.InProgress;
			receiptEdoTask.ReceiptStatus = EdoReceiptStatus.Sending;
			receiptEdoTask.StartTime = DateTime.Now;
			receiptEdoTask.CashboxId = receiptEdoTask.OrderEdoRequest.Order.Contract.Organization.CashBoxId;
			await _uow.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			var sendReceiptMessage = new ReceiptReadyToSendEvent { ReceiptEdoTaskId = receiptEdoTask.Id };
			await _messageBus.Publish(sendReceiptMessage);
		}


		private async Task SaveCodesToPool(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			foreach(var taskItem in receiptEdoTask.Items)
			{
				var code = taskItem.ProductCode.SourceCode;
				if(code.IsInvalid)
				{
					continue;
				}

				await _trueMarkCodesPool.PutCodeAsync(code.Id, cancellationToken);
			}
		}

		private async Task PrepareReceipt(
			ReceiptEdoTask receiptEdoTask,
			EdoTaskItemTrueMarkStatusProvider trueMarkCodesChecker,
			CancellationToken cancellationToken)
		{
			await PrepareFiscalDocuments(receiptEdoTask, cancellationToken);

			TrueMarkTaskValidationResult taskValidationResult;
			bool isValid = true;
			int attempts = 5;

			do
			{
				if(!isValid)
				{
					attempts--;
					await PrepareFiscalDocuments(receiptEdoTask, cancellationToken);
					trueMarkCodesChecker.ClearCache();
				}

				// проверяем все коды по задаче в ЧЗ
				trueMarkCodesChecker.ClearCache();
				taskValidationResult = await _localCodesValidator.ValidateAsync(
					receiptEdoTask, 
					trueMarkCodesChecker, 
					cancellationToken
				);
				isValid = taskValidationResult.IsAllValid;
				if(!isValid)
				{
					var hasGroupInvalidCodes = false;
					// очистка result кодов не валидных позиций
					foreach(var codeResult in taskValidationResult.CodeResults)
					{
						if(codeResult.IsValid)
						{
							continue;
						}

						if(codeResult.EdoTaskItem.ProductCode.ResultCode == null)
						{
							continue;
						}

						// определить что TaskItem принадлежит групповому коду
						if(codeResult.EdoTaskItem.ProductCode.ResultCode.ParentWaterGroupCodeId != null)
						{
							hasGroupInvalidCodes = true;

							// так же надо зачистить все части этого группового кода
							var groupCode = _trueMarkCodeRepository.GetParentGroupCode(_uow, codeResult.EdoTaskItem.ProductCode.ResultCode.ParentWaterGroupCodeId.Value);
							foreach(var individualCode in groupCode.GetAllCodes().Where(x => x.IsTrueMarkWaterIdentificationCode))
							{
								var foundInvalidGroupIdentificationCode = receiptEdoTask.Items
									.FirstOrDefault(x => x.ProductCode.SourceCode == individualCode.TrueMarkWaterIdentificationCode);
								foundInvalidGroupIdentificationCode.ProductCode.SourceCode = null;
								foundInvalidGroupIdentificationCode.ProductCode.ResultCode = null;
							}
						}
						else
						{
							var gtin = await _uow.Session.QueryOver<GtinEntity>()
								.Where(x => x.GtinNumber == codeResult.EdoTaskItem.ProductCode.ResultCode.GTIN)
								.SingleOrDefaultAsync(cancellationToken);
							var newCode = await LoadCodeFromPool(gtin, cancellationToken);
							codeResult.EdoTaskItem.ProductCode.ResultCode = newCode;
						}
					}

					continue;
				}

				if(!taskValidationResult.ReadyToSell)
				{
					// создание заявок на трансфер
					var iteration = await _transferRequestCreator.CreateTransferRequests(
						_uow, 
						receiptEdoTask, 
						trueMarkCodesChecker, 
						cancellationToken
					);

					receiptEdoTask.ReceiptStatus = EdoReceiptStatus.Transfering;

					await _uow.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
					await _uow.CommitAsync(cancellationToken);

					var receiptTransferMessage = new TransferRequestCreatedEvent { TransferIterationId = iteration.Id };
					await _messageBus.Publish(receiptTransferMessage);
					return;
				}

				// итоговая валидация и получение разрешительного режима
				var industryRequisitePrepareResult = await PrepareIndustryRequisite(receiptEdoTask, cancellationToken);
				switch(industryRequisitePrepareResult)
				{
					case IndustryRequisitePrepareResult.Succeeded:
						// Ничего не делаем, продолжаем
						break;
					case IndustryRequisitePrepareResult.NeedToChange:
						isValid = false;
						continue;
					case IndustryRequisitePrepareResult.Problem:
						return;
					default:
						throw new NotSupportedException($"Неизвестный результат подготовки разрешительного " +
							$"режима: {industryRequisitePrepareResult}");
				}

				if(attempts == 0)
				{
					await _edoProblemRegistrar.RegisterCustomProblem<ReceiptPrepareMaxAttemptsReached>(
						receiptEdoTask, 
						cancellationToken);
					return;
				}

			} while(!isValid && attempts > 0);

			if(!isValid)
			{
				await _edoProblemRegistrar.RegisterCustomProblem<ReceiptPrepareMaxAttemptsReached>(
						receiptEdoTask,
						cancellationToken);
				return;
			}

			if(!receiptEdoTask.FiscalDocuments.Any())
			{
				throw new InvalidOperationException("Проблема с подготовкой фискальных документов. Не удалось создать ни один документ.");
			}

			// перевод в отправку
			receiptEdoTask.Status = EdoTaskStatus.InProgress;
			receiptEdoTask.ReceiptStatus = EdoReceiptStatus.Sending;
			receiptEdoTask.StartTime = DateTime.Now;
			receiptEdoTask.CashboxId = receiptEdoTask.OrderEdoRequest.Order.Contract.Organization.CashBoxId;
			await _uow.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			var sendReceiptMessage = new ReceiptReadyToSendEvent { ReceiptEdoTaskId = receiptEdoTask.Id };
			await _messageBus.Publish(sendReceiptMessage);
		}

		/// <summary>
		/// Создает фискальные документы
		/// Подготавливает коды
		/// </summary>
		/// <param name="receiptEdoTask"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private async Task PrepareFiscalDocuments(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			var order = receiptEdoTask.OrderEdoRequest.Order;

			//получаем продуктовые коды, но только те, в которые не входят консолидированные идентификационные коды
			var receiptEdoTaskProductCodesWithoutConsolidated = GetProductCodesWithoutConsolidatedIdentificationCodes(receiptEdoTask.Items);

			//проверяем продуктовые коды на дубликаты, если дубли найдены, то меняем статус, проблему и устанавливаем кол-во дублей
			CheckProductCodesForDuplicatesAndUpdateIfNeed(receiptEdoTaskProductCodesWithoutConsolidated);

			//создать или обновить немаркированные позиции
			var mainFiscalDocument = UpdateUnmarkedFiscalDocument(receiptEdoTask);
			await UpdateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, cancellationToken);

			//создать или обновить сумму в чеках
			foreach(var fiscalDocument in receiptEdoTask.FiscalDocuments)
			{
				UpdateReceiptMoneyPositions(fiscalDocument);
			}
		}

		public List<TrueMarkProductCode> GetProductCodesWithoutConsolidatedIdentificationCodes(IEnumerable<EdoTaskItem> edoTaskItems)
		{
			return edoTaskItems
				.Select(x => x.ProductCode)
				.Where(x => x.ResultCode == null)
				.Where(x => x.SourceCode != null)
				.Where(x => x.SourceCode.ParentWaterGroupCodeId == null)
				.ToList();
		}

		public void CheckProductCodesForDuplicatesAndUpdateIfNeed(IEnumerable<TrueMarkProductCode> productCodes)
		{
			var sourceAndResultCodesIds = productCodes.Select(x => x.SourceCode.Id)
				.Concat(productCodes.Where(x => x.ResultCode != null).Select(x => x.ResultCode.Id))
				.Distinct()
				.ToList();

			var existingProductCodesByIds = GetProductCodesHavingRequiredResultCodeIds(sourceAndResultCodesIds);

			foreach(var productCode in productCodes)
			{
				if(productCode.ResultCode != null)
				{
					existingProductCodesByIds.TryGetValue(productCode.ResultCode.Id, out var productCodesByResultCodeId);

					if(productCodesByResultCodeId?.Any(x => x.Id != productCode.Id) != true)
					{
						continue;
					}

					productCode.ResultCode = null;
				}

				existingProductCodesByIds.TryGetValue(productCode.SourceCode.Id, out var productCodesBySourceCodeId);

				var duplicatesCount = productCodesBySourceCodeId?.Count(x => x.Id != productCode.Id) ?? 0;

				if(duplicatesCount > 0)
				{
					productCode.SourceCodeStatus = SourceProductCodeStatus.Problem;
					productCode.Problem = ProductCodeProblem.Duplicate;
					productCode.DuplicatesCount = duplicatesCount;
				}
			}
		}

		private IDictionary<int, List<TrueMarkProductCode>> GetProductCodesHavingRequiredResultCodeIds(IEnumerable<int> resultCodeIds)
		{
			return _productCodeRepository
				.Get(_uow, x => resultCodeIds.Contains(x.ResultCode.Id))
				.GroupBy(x => x.ResultCode.Id)
				.ToDictionary(x => x.Key, x => x.ToList());
		}

		public EdoFiscalDocument UpdateUnmarkedFiscalDocument(ReceiptEdoTask receiptEdoTask)
		{
			var order = receiptEdoTask.OrderEdoRequest.Order;
			var pricedOrderItems = order.OrderItems
				.Where(x => x.Price != 0m)
				.Where(x => x.Count > 0m);
			var unmarkedOrderItems = pricedOrderItems
				.Where(x => x.Nomenclature.IsAccountableInTrueMark == false);

			var fiscalDocument = PrepareFiscalDocument(receiptEdoTask, 0);

			fiscalDocument.InventPositions.Clear();
			fiscalDocument.MoneyPositions.Clear();

			//обновление не маркированных позиций
			foreach(var unmarkedOrderItem in unmarkedOrderItems)
			{
				var inventPosition = CreateInventPosition(unmarkedOrderItem);
				inventPosition.Quantity = unmarkedOrderItem.Count;
				inventPosition.DiscountSum = unmarkedOrderItem.DiscountMoney;

				fiscalDocument.InventPositions.Add(inventPosition);
			}

			return fiscalDocument;
		}

		public async Task UpdateMarkedFiscalDocuments(
			ReceiptEdoTask receiptEdoTask, 
			EdoFiscalDocument mainFiscalDocument, 
			CancellationToken cancellationToken
			)
		{
			var order = receiptEdoTask.OrderEdoRequest.Order;
			var markedOrderItems = order.OrderItems
				.Where(x => x.Price != 0m)
				.Where(x => x.Count > 0m)
				.Where(x => x.Nomenclature.IsAccountableInTrueMark == true);

			var expandedMarkedItems = ExpandMarkedOrderItems(markedOrderItems).ToList();
			var unprocessedCodes = receiptEdoTask.Items.ToList();


			// ОБРАБОТКА ГРУППОВЫХ КОДОВ

			// отобрали от списка необработанных кодов все групповые коды
			// их обработаем в первую очередь
			var groupCodesWithTaskItems = TakeGroupCodesWithTaskItems(unprocessedCodes);

			var groupFiscalInventPositions = new List<FiscalInventPosition>();

			foreach(var groupCodeWithTaskItems in groupCodesWithTaskItems.ToList())
			{
				var groupCode = groupCodeWithTaskItems.Key;
				var affectedTaskItems = groupCodeWithTaskItems.Value;

				var individualCodesInGroupCount = affectedTaskItems.Count();

				// знаем кол-во кодов в группе
				// теперь нужно создать позицию в чек на соответствующее кол-во

				// найти товары в заказе подходящие по GTIN группового кода
				var availableOrderItems = expandedMarkedItems
					.Where(x => x.OrderItem.Nomenclature.GroupGtins.Any(g => g.GtinNumber == groupCode.GTIN));

				// группируем распределнные товары заказа обратно по одному orderItem
				// чтобы мы могли назначить групповой код на определенный orderItem, в котором 
				// имеется достаточное кол-во товаров для группового кода
				var groupedByOrderItem = availableOrderItems.GroupBy(x => x.OrderItem.Id);
				foreach(var expandedOrderItemsForOrderItem in groupedByOrderItem)
				{
					var orderItem = expandedOrderItemsForOrderItem.First().OrderItem;

					var expandedOrderItemsForOrderItemList = expandedOrderItemsForOrderItem.ToList();
					var orderItemsCount = expandedOrderItemsForOrderItemList.Count;
					if(orderItemsCount < individualCodesInGroupCount)
					{
						// если кол-во товаров в OrderItem меньше чем кодов в группе, то
						// продолжаем искать другие OrderItem, где будет достаточное кол-во товаров
						continue;
					}

					var inventPosition = CreateInventPosition(orderItem);

					// i использовать не надо, цикл нужен только для того чтобы прибавить позиции
					// нужное кол-во раз, соответствующее кол-ву кодов в группе
					for(int i = 0; i < individualCodesInGroupCount; i++)
					{
						var firstAvailableExpandedOrderItem = expandedOrderItemsForOrderItemList.First();

						// делаем инкремент потомучто expandedOrderItem соответствует одной единице товара в OrderItem
						inventPosition.Quantity++;
						// добавляем пропроциональную скидку для одной еденицы товара, которая была ранее рассчитана
						// при распределении товаров заказа на их кол-во в каждом товаре
						inventPosition.DiscountSum += firstAvailableExpandedOrderItem.DiscountPerSingleItem;


						// исключаем обработанный товар из первоначального списка распределенных товаров
						// чтобы при обработке следующей группы, этот товар не попал под обработку, потому
						// что мы его уже назначили на определенную группу и забрали от него сумму пропорциональной скидки
						expandedMarkedItems.Remove(firstAvailableExpandedOrderItem);

						// исключаем обработанный товар из списка распределенных товаров на текущем OrderItem, для того
						// чтобы на следующей итерации цикла for мы не обработали его еще раз
						expandedOrderItemsForOrderItemList.Remove(firstAvailableExpandedOrderItem);
					}

					inventPosition.EdoTaskItem = null;
					inventPosition.GroupCode = groupCode;

					groupFiscalInventPositions.Add(inventPosition);

					// убираем назначенный групповой код из списка, чтобы потом увидеть не назначенные остатки
					// и обработать их отдельно, другим способом
					groupCodesWithTaskItems.Remove(groupCode);
					break;
				}
			}

			// оставшиемся группы распределяем на любые товары в заказе 
			// без жесткой привязки к конкретному OrderItem
			// но в InventPosition будет указан только первый OrderItem

			foreach(var remainGroupCodeItem in groupCodesWithTaskItems)
			{
				var groupCode = remainGroupCodeItem.Key;
				var individualCodesInGroupCount = remainGroupCodeItem.Value.Count();

				// знаем кол-во кодов в группе
				// теперь нужно создать позицию в чек на соответствующее кол-во

				// найти товары в заказе подходящие по GTIN группового кода
				var orderItemsForInventoryPosition = expandedMarkedItems
					.Where(x => x.OrderItem.Nomenclature.GroupGtins.Any(g => g.GtinNumber == groupCode.GTIN))
					.Take(individualCodesInGroupCount)
					.ToList();

				if(orderItemsForInventoryPosition.Count < individualCodesInGroupCount)
				{
					_logger.LogWarning("Для группового кода Id {groupCodeId} GTIN {groupCodeGTIN} не хватает товаров в заказе.",
						groupCode.Id, groupCode.GTIN);

					continue;
				}

				var orderItemsForInventoryPositionPricesSum =
					orderItemsForInventoryPosition.Sum(x => x.OrderItem.Price);

				var orderItemsForInventoryPositionDiscountsSum =
					orderItemsForInventoryPosition.Sum(x => x.DiscountPerSingleItem);

				//Округляем цену за единицу до копееек в большую стороную. Далее при необходимости увеличим сумму скидки
				var pricePerItem = Math.Ceiling(100 * orderItemsForInventoryPositionPricesSum / individualCodesInGroupCount) / 100;

				var inventPosition = CreateInventPosition(orderItemsForInventoryPosition.Select(x => x.OrderItem), pricePerItem);
				inventPosition.Quantity = orderItemsForInventoryPosition.Count;
				inventPosition.EdoTaskItem = null;
				inventPosition.GroupCode = groupCode;
				inventPosition.DiscountSum =
					orderItemsForInventoryPositionDiscountsSum + (inventPosition.Price * inventPosition.Quantity - orderItemsForInventoryPositionPricesSum);

				groupFiscalInventPositions.Add(inventPosition);

				foreach(var orderItemInInventoyPosition in orderItemsForInventoryPosition)
				{
					// исключаем обработанный товар из первоначального списка распределенных товаров
					// чтобы при обработке следующей группы, этот товар не попал под обработку, потому
					// что мы его уже назначили на определенную группу и забрали от него сумму пропорциональной скидки
					expandedMarkedItems.Remove(orderItemInInventoyPosition);
				}
			}

			// РАСПРЕДЕЛЕНИЕ ГРУППОВЫХ InventPosition НА ФИСКАЛЬНЫЕ ДОКУМЕНТЫ
			var documentIndex = mainFiscalDocument.Index;
			var currentFiscalDocument = mainFiscalDocument;
			var currentProcessingGroupPositions = groupFiscalInventPositions.Skip(0).Take(_maxCodesInReceipt);
			var lastGroupFiscalInventPositionsCount = 0;
			do
			{
				// записываем сколько было добавлено позиций в последнем документе
				// чтобы дополнить документ до максимального кол-ва позиций в обработке индивидуальных кодов
				lastGroupFiscalInventPositionsCount = currentProcessingGroupPositions.Count();
				if(lastGroupFiscalInventPositionsCount == 0)
				{
					break;
				}

				foreach(var processingGroupPosition in currentProcessingGroupPositions)
				{
					currentFiscalDocument.InventPositions.Add(processingGroupPosition);
				}

				currentProcessingGroupPositions = groupFiscalInventPositions
						.Skip(_maxCodesInReceipt * documentIndex + 1)
						.Take(_maxCodesInReceipt);

				// подготавливаем данные для следующей итерации
				if(currentProcessingGroupPositions.Any())
				{
					documentIndex++;
					currentFiscalDocument = PrepareFiscalDocument(receiptEdoTask, documentIndex);
				}

			} while(currentProcessingGroupPositions.Any());


			// ОБРАБОТКА ИНДИВИДУАЛЬНЫХ КОДОВ

			var processedPositions = expandedMarkedItems.ToList();

			var currentProcessingPositions = processedPositions
				// выбираем то кол-во позиций которое не хватает до максимального
				// кол-ва позиций в текущем фискальном документе
				.Take(_maxCodesInReceipt - lastGroupFiscalInventPositionsCount)
				.ToList();

			do
			{
				if(!currentProcessingPositions.Any())
				{
					break;
				}

				// заполняем товарами с кодами текущий документ
				foreach(var processingPosition in currentProcessingPositions)
				{
					var inventPosition = await PrepareMarkedInventPosition(
						receiptEdoTask,
						processingPosition.OrderItem,
						unprocessedCodes,
						cancellationToken
					);
					inventPosition.DiscountSum = processingPosition.DiscountPerSingleItem;

					currentFiscalDocument.InventPositions.Add(inventPosition);
					processedPositions.Remove(processingPosition);

				}

				// подготавливаем данные для следующей итерации
				currentProcessingPositions = processedPositions
					.Take(_maxCodesInReceipt)
					.ToList();

				if(currentProcessingPositions.Any())
				{
					documentIndex++;
					currentFiscalDocument = PrepareFiscalDocument(receiptEdoTask, documentIndex);
				}

			} while(currentProcessingPositions.Any());

			// Сохранение остатков в пул
			foreach(var unprocessedCode in unprocessedCodes)
			{
				if(unprocessedCode.ProductCode.SourceCode != null)
				{
					await _trueMarkCodesPool.PutCodeAsync(unprocessedCode.ProductCode.SourceCode.Id, cancellationToken);
				}
				receiptEdoTask.Items.Remove(unprocessedCode);
				await _uow.DeleteAsync(unprocessedCode, cancellationToken);
			}
		}		

		private IDictionary<TrueMarkWaterGroupCode, IEnumerable<EdoTaskItem>> TakeGroupCodesWithTaskItems(List<EdoTaskItem> unprocessedTaskItems)
		{
			// нашли все индивидуальные коды, которые содержатся в группах
			var codesThatContainedInGroup = unprocessedTaskItems
				.Where(x => x.ProductCode.SourceCode != null)
				.Where(x => x.ProductCode.SourceCode.IsInvalid == false)
				.Where(x => x.ProductCode.SourceCode.ParentWaterGroupCodeId != null)
				.ToList()
				;

			// исключили из обрабатываемого списка все коды, которые содержатся в группах
			// они не подходят для индивидуальной обработки, потому что не имеют CheckCode
			unprocessedTaskItems.RemoveAll(x => codesThatContainedInGroup.Contains(x));

			var groupped = codesThatContainedInGroup
				.GroupBy(x => x.ProductCode.SourceCode.ParentWaterGroupCodeId);

			var parentCodesIds = groupped
				.Select(x => x.Key)
				.Distinct();

			var parentCodes = parentCodesIds
				.Select(x => _trueMarkCodeRepository.GetParentGroupCode(_uow, x.Value))
				.Distinct();

			var result = new Dictionary<TrueMarkWaterGroupCode, IEnumerable<EdoTaskItem>>();

			foreach(var parentCode in parentCodes)
			{
				result.Add(parentCode, codesThatContainedInGroup
					.Where(ctcig => parentCode
						.GetAllCodes()
						.Where(x => x.IsTrueMarkWaterIdentificationCode)
						.Select(x => x.TrueMarkWaterIdentificationCode)
						.Any(x => x.Id == ctcig.ProductCode.SourceCode.Id)));
			}

			// нашли все групповые коды

			return result;
		}

		private void UpdateReceiptMoneyPositions(EdoFiscalDocument currentFiscalDocument)
		{
			var order = currentFiscalDocument.ReceiptEdoTask.OrderEdoRequest.Order;

			var receiptSum = currentFiscalDocument.InventPositions
				.Sum(x =>  x.Price * x.Quantity - x.DiscountSum);

			var moneyPosition = new FiscalMoneyPosition
			{
				PaymentType = GetPaymentType(order.PaymentType),
				Sum = receiptSum
			};

			currentFiscalDocument.MoneyPositions.Clear();
			currentFiscalDocument.MoneyPositions.Add(moneyPosition);
		}

		private FiscalPaymentType GetPaymentType(PaymentType orderPaymentType)
		{
			switch(orderPaymentType)
			{
				case PaymentType.Terminal:
				case PaymentType.DriverApplicationQR:
				case PaymentType.SmsQR:
				case PaymentType.PaidOnline:
					return FiscalPaymentType.Card;
				default:
					return FiscalPaymentType.Cash;
			}
		}

		private IEnumerable<(OrderItemEntity OrderItem, decimal DiscountPerSingleItem)> ExpandMarkedOrderItems(IEnumerable<OrderItemEntity> markedOrderItems)
		{
			// предоставляет каждую единицу товара отдельным элементом
			// с рассчитанной пропорциональной скидкой
			var expandedMarkedItems = markedOrderItems.SelectMany(orderItem =>
			{
				var multipliedItems = new List<(OrderItemEntity OrderItem, decimal DiscountPerSingleItem)>();

				decimal wholeDiscount = 0;
				//i == 1 чтобы пропуcтить последний элемент, у него расчет происходит из остатков
				for(int i = 1; i < orderItem.Count; i++)
				{
					var itemDiscount = 0m;
					if(wholeDiscount < orderItem.DiscountMoney)
					{
						var partDiscount = Math.Round(orderItem.DiscountMoney / orderItem.Count, 1);
						wholeDiscount += partDiscount;
						itemDiscount = partDiscount;
					}
					multipliedItems.Add((orderItem, itemDiscount));
				}

				//добавление последнего элемента с остатками от целой скидки
				var residueDiscount = orderItem.DiscountMoney - wholeDiscount;
				if(residueDiscount < 0)
				{
					residueDiscount = 0;
				}
				multipliedItems.Add((orderItem, residueDiscount));

				return multipliedItems;
			});
			return expandedMarkedItems;
		}

		/// <summary>
		/// Создает и подготавливает инвентарную позицию для одного экземпляра товара <br/>
		/// Сопоставляет по Gtin товара отсканированные коды и выбирает подходящий <br/>
		/// Создает новый код если не нашел подходящий <br/>
		/// Не подходящие коды заменяются кодами из пула
		/// </summary>
		/// <param name="unprocessedCodes">Список всех отсканированных кодов еще необработанных данным методом,
		/// после подбора код исключается из него</param>
		private async Task<FiscalInventPosition> PrepareMarkedInventPosition(
			ReceiptEdoTask receiptEdoTask,
			OrderItemEntity orderItem,
			List<EdoTaskItem> unprocessedCodes,
			CancellationToken cancellationToken
			)
		{
			EdoTaskItem matchEdoTaskItem = null;
			int codeIdFromPool;

			var inventPosition = CreateInventPosition(orderItem);
			inventPosition.Quantity = 1;
			inventPosition.OrderItems = new ObservableList<OrderItemEntity> { orderItem };

			// Пытаемся найти совпадающий по Gtin код:

			// сначала у кого заполнен Result код
			var resultCodes = unprocessedCodes
				.Where(x => x.ProductCode.Problem == ProductCodeProblem.None)
				.Where(x => x.ProductCode.ResultCode != null);
			foreach(var gtin in orderItem.Nomenclature.Gtins)
			{
				matchEdoTaskItem = resultCodes
					.Where(x => x.ProductCode.ResultCode.GTIN == gtin.GtinNumber)
					.FirstOrDefault();
				if(matchEdoTaskItem != null)
				{
					inventPosition.EdoTaskItem = matchEdoTaskItem;
					unprocessedCodes.Remove(matchEdoTaskItem);
					return inventPosition;
				}
			}

			// затем у кого заполнен Source код без проблем

			var sourceCodes = unprocessedCodes
				.Where(x => x.ProductCode.Problem == ProductCodeProblem.None)
				.Where(x => x.ProductCode.ResultCode == null)
				.Where(x => x.ProductCode.SourceCode != null)
				.Where(x => x.ProductCode.SourceCode.IsInvalid == false);
			foreach(var gtin in orderItem.Nomenclature.Gtins)
			{
				matchEdoTaskItem = sourceCodes
					.Where(x => x.ProductCode.SourceCode.GTIN == gtin.GtinNumber)
					.FirstOrDefault();
				if(matchEdoTaskItem != null)
				{
					matchEdoTaskItem.ProductCode.ResultCode = matchEdoTaskItem.ProductCode.SourceCode;
					matchEdoTaskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Accepted;
					await _uow.SaveAsync(matchEdoTaskItem, cancellationToken: cancellationToken);

					inventPosition.EdoTaskItem = matchEdoTaskItem;
					unprocessedCodes.Remove(matchEdoTaskItem);
					return inventPosition;
				}
			}

			// затем у кого заполнен Source код, но дефектный или задублированный
			// и заполняем у него Result кодом из пула
			var ddCodes = unprocessedCodes
				.Where(x => x.ProductCode.Problem.IsIn(ProductCodeProblem.Defect, ProductCodeProblem.Duplicate))
				.Where(x => x.ProductCode.ResultCode == null)
				.Where(x => x.ProductCode.SourceCode != null)
				.Where(x => x.ProductCode.SourceCode.IsInvalid = false);
			foreach(var gtin in orderItem.Nomenclature.Gtins)
			{
				matchEdoTaskItem = ddCodes
					.Where(x => x.ProductCode.SourceCode.GTIN == gtin.GtinNumber)
					.FirstOrDefault();
				if(matchEdoTaskItem != null)
				{
					// запись кода из пула в result
					var identificationCode = await LoadCodeFromPool(gtin, cancellationToken);
					matchEdoTaskItem.ProductCode.ResultCode = identificationCode;
					matchEdoTaskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;
					await _uow.SaveAsync(matchEdoTaskItem, cancellationToken: cancellationToken);

					inventPosition.EdoTaskItem = matchEdoTaskItem;
					unprocessedCodes.Remove(matchEdoTaskItem);
					return inventPosition;
				}
			}

			// затем берем первый оставшийся не валидный код
			// и заполняем у него Result кодом из пула
			var invalidCodes = unprocessedCodes
				.Where(x => x.ProductCode.SourceCode == null || x.ProductCode.SourceCode.IsInvalid);
			//Reverse чтобы брать свежий Gtin, раз уж заполняем новым кодом
			foreach(var gtin in orderItem.Nomenclature.Gtins.Reverse())
			{
				matchEdoTaskItem = invalidCodes.FirstOrDefault();
				if(matchEdoTaskItem != null)
				{
					// запись кода из пула в result
					var identificationCode = await LoadCodeFromPool(gtin, cancellationToken);
					matchEdoTaskItem.ProductCode.ResultCode = identificationCode;
					matchEdoTaskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;
					await _uow.SaveAsync(matchEdoTaskItem, cancellationToken: cancellationToken);

					inventPosition.EdoTaskItem = matchEdoTaskItem;
					unprocessedCodes.Remove(matchEdoTaskItem);
					return inventPosition;
				}
			}

			// Если ничего не смогли подобрать, то создаем новую запись
			var code = await LoadCodeFromPool(orderItem.Nomenclature, cancellationToken);

			var newProductCode = new AutoTrueMarkProductCode
			{
				Problem = ProductCodeProblem.Unscanned,
				CustomerEdoRequest = receiptEdoTask.OrderEdoRequest,
				SourceCodeStatus = SourceProductCodeStatus.Changed,
				SourceCode = null,
				ResultCode = code
			};

			await _uow.SaveAsync(newProductCode, cancellationToken: cancellationToken);
			matchEdoTaskItem = new EdoTaskItem
			{
				CustomerEdoTask = receiptEdoTask,
				ProductCode = newProductCode
			};

			await _uow.SaveAsync(matchEdoTaskItem, cancellationToken: cancellationToken);
			receiptEdoTask.Items.Add(matchEdoTaskItem);

			inventPosition.EdoTaskItem = matchEdoTaskItem;
			return inventPosition;
		}

		private async Task<TrueMarkWaterIdentificationCode> LoadCodeFromPool(NomenclatureEntity nomenclature, CancellationToken cancellationToken)
		{
			int codeId = 0;
			var problemGtins = new List<EdoProblemCustomItem>();
			EdoCodePoolMissingCodeException exception = null;

			foreach(var gtin in nomenclature.Gtins.Reverse())
			{
				try
				{
					codeId = await _trueMarkCodesPool.TakeCode(gtin.GtinNumber, cancellationToken);
				}
				catch(EdoCodePoolMissingCodeException ex) 
				{
					exception = ex;
					problemGtins.Add(new EdoProblemGtinItem
					{
						Gtin = gtin
					});
				}
			}

			if(codeId == 0)
			{
				throw new EdoProblemException(exception, problemGtins);
			}

			return await _uow.Session.GetAsync<TrueMarkWaterIdentificationCode>(codeId, cancellationToken);
		}

		private async Task<TrueMarkWaterIdentificationCode> LoadCodeFromPool(GtinEntity gtin, CancellationToken cancellationToken)
		{
			int codeId = 0;
			var problemGtins = new List<EdoProblemCustomItem>();
			EdoCodePoolMissingCodeException exception = null;

			try
			{
				codeId = await _trueMarkCodesPool.TakeCode(gtin.GtinNumber, cancellationToken);
			}
			catch(EdoCodePoolMissingCodeException ex)
			{
				exception = ex;
				problemGtins.Add(new EdoProblemGtinItem
				{
					Gtin = gtin
				});
			}

			if(codeId == 0)
			{
				throw new EdoProblemException(exception, problemGtins);
			}

			return await _uow.Session.GetAsync<TrueMarkWaterIdentificationCode>(codeId, cancellationToken);
		}

		private enum IndustryRequisitePrepareResult
		{
			Succeeded,
			NeedToChange,
			Problem
		}

		private async Task<IndustryRequisitePrepareResult> PrepareIndustryRequisite(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			var seller = receiptEdoTask.OrderEdoRequest.Order.Contract.Organization;
			var cashBoxToken = seller.CashBoxTokenFromTrueMark;
			if(cashBoxToken == null)
			{
				await _edoProblemRegistrar.RegisterCustomProblem<IndustryRequisiteMissingOrganizationToken>(
					receiptEdoTask,
					cancellationToken,
					$"Отсутствует токен для организации Id {seller.Id}");
				return IndustryRequisitePrepareResult.Problem;
			}

			var regulatoryDocument = _uow.GetById<FiscalIndustryRequisiteRegulatoryDocument>(_edoReceiptSettings.IndustryRequisiteRegulatoryDocumentId);
			if(regulatoryDocument == null)
			{
				await _edoProblemRegistrar.RegisterCustomProblem<IndustryRequisiteRegualtoryDocumentIsMissing>(
					receiptEdoTask,
					cancellationToken);
				return IndustryRequisitePrepareResult.Problem;
			}

			bool isValid = true;

			foreach(var fiscalDocument in receiptEdoTask.FiscalDocuments)
			{
				var codesToCheck1260 = fiscalDocument.InventPositions
					.Where(x => x.EdoTaskItem?.ProductCode != null || x.GroupCode != null)
					.ToDictionary(x =>
					{
						if(x.EdoTaskItem != null)
						{
							return x.EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;
						}
						
						return x.GroupCode.FormatForCheck1260;
					});

				if(!codesToCheck1260.Any())
				{
					continue;
				}

				var result = await _tag1260Checker.CheckCodesForTag1260Async(
					codesToCheck1260.Keys, 
					cashBoxToken.Value, 
					cancellationToken
				);

				if(result.Code != 0)
				{
					await _edoProblemRegistrar.RegisterCustomProblem<IndustryRequisiteCheckApiError>(
						receiptEdoTask,
						cancellationToken,
						$"Код ошибки: {result.Code}, сообщение: {result.Description}");
					return IndustryRequisitePrepareResult.Problem;
				}

				var invalidCodes = result.Codes.Where(codeResult =>
				{
					var canSell = codeResult.ErrorCode == 0
						&& codeResult.Found
						&& codeResult.Valid
						&& codeResult.Verified
						&& codeResult.ExpireDate > DateTime.Now
						&& codeResult.Realizable
						&& codeResult.Utilised
						&& !codeResult.IsBlocked
						&& !codeResult.Sold;
					return !canSell;
				});

				if(invalidCodes.Any())
				{
					isValid = false;
					continue;
				}

				foreach(var codeResult in result.Codes)
				{
					var inventPosition = codesToCheck1260[codeResult.Cis];
					inventPosition.IndustryRequisiteData = $"UUID={result.ReqId}&Time={result.ReqTimestamp}";
					inventPosition.RegulatoryDocument = regulatoryDocument;
					await _uow.SaveAsync(inventPosition, cancellationToken: cancellationToken);
				}
			}

			if(isValid)
			{
				_edoProblemRegistrar.SolveCustomProblem<IndustryRequisiteMissingOrganizationToken>(receiptEdoTask);
				_edoProblemRegistrar.SolveCustomProblem<IndustryRequisiteRegualtoryDocumentIsMissing>(receiptEdoTask);
				_edoProblemRegistrar.SolveCustomProblem<IndustryRequisiteCheckApiError>(receiptEdoTask);
				return IndustryRequisitePrepareResult.Succeeded;
			}
			else
			{
				return IndustryRequisitePrepareResult.NeedToChange;
			}
		}

		private EdoFiscalDocument PrepareFiscalDocument(ReceiptEdoTask receiptEdoTask, int documentIndex)
		{
			var order = receiptEdoTask.OrderEdoRequest.Order;
			var fiscalDocument = receiptEdoTask.FiscalDocuments.FirstOrDefault(x => x.Index == documentIndex);

			if(fiscalDocument == null)
			{
				var documentNumber = documentIndex > 0 
					? $"vod_{order.Id}_{documentIndex}" 
					: $"vod_{order.Id}";
				 
				fiscalDocument = new EdoFiscalDocument
				{
					ReceiptEdoTask = receiptEdoTask,
					Stage = FiscalDocumentStage.Preparing,
					Status = FiscalDocumentStatus.None,
					DocumentGuid = Guid.NewGuid(),
					DocumentNumber = documentNumber,
					DocumentType = FiscalDocumentType.Sale,
					CheckoutTime = order.TimeDelivered ?? DateTime.Now,
					Contact = GetContact(order),
					//Для собственных нужд не заполняется
					ClientInn = null,
					CashierName = order.Contract?.Organization?.ActiveOrganizationVersion?.Leader?.ShortName,
					//По умолчанию не печатаем чеки
					PrintReceipt = false,
					Index = documentIndex
				};
				receiptEdoTask.FiscalDocuments.Add(fiscalDocument);
			}
			else
			{
				if(documentIndex > 0)
				{
					fiscalDocument.InventPositions.Clear();
					fiscalDocument.MoneyPositions.Clear();
				}
			}
				
			return fiscalDocument;
		}

		private FiscalInventPosition CreateInventPosition(OrderItemEntity orderItem)
		{
			return CreateInventPosition(new List<OrderItemEntity> { orderItem }, Math.Round(orderItem.Price, 2));
		}
		
		private FiscalInventPosition CreateInventPosition(IEnumerable<OrderItemEntity> orderItems, decimal pricePerItem)
		{
			if(orderItems.Select(x => x.Order.Id).Distinct().Count() > 1)
			{
				throw new InvalidOperationException("Нельзя создать товар в чеке для строк разных заказов");
			}

			if(orderItems.Select(x => x.Nomenclature.Id).Distinct().Count() > 1)
			{
				throw new InvalidOperationException("Нельзя создать товар в чеке для строк заказа с разной номенклатурой");
			}

			var nomenclature = orderItems.First().Nomenclature;
			var order = orderItems.First().Order;

			var inventPosition = new FiscalInventPosition
			{
				Name = nomenclature.OfficialName,
				Price = pricePerItem,
				OrderItems = new ObservableList<OrderItemEntity>(orderItems)
			};

			var organization = order.Contract?.Organization;

			if(organization is null || organization.WithoutVAT || nomenclature.VAT == VAT.No)
			{
				inventPosition.Vat = FiscalVat.VatFree;
			}
			else
			{
				inventPosition.Vat = FiscalVat.Vat20;
			}

			return inventPosition;
		}

		private async Task<bool> HasReceiptOnSumToday(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			var sum = receiptEdoTask.OrderEdoRequest.Order.OrderItems
				.Where(x => x.Count > 0)
				.Sum(x => x.Sum);

			var hasReceipt = await _edoRepository.HasReceiptOnSumToday(sum, cancellationToken);
			return hasReceipt;
		}

		/// <summary>
		/// Возврат первого попавшегося контакта из цепочки:<br/>
		/// 0. Почта для чеков в контрагенте<br/>
		/// 1. Почта для счетов в контрагенте<br/>
		/// 2. Телефон для чеков в точке доставки<br/>
		/// 3. Телефон для чеков в контрагенте<br/>
		/// 4. Телефон личный в ТД<br/>
		/// 5. Телефон личный в контрагенте<br/>
		/// 6. Иная почта в контрагенте<br/>
		/// 7. Городской телефон в ТД<br/>
		/// 8. Городской телефон в контрагенте<br/>
		/// </summary>
		/// <returns>Контакт с минимальным весом.<br/>Телефоны возвращает в формате +7</returns>
		public virtual string GetContact(OrderEntity order)
		{
			if(order.Client == null)
			{
				return null;
			}

			//Dictionary<вес контакта, контакт>
			Dictionary<int, string> contacts = new Dictionary<int, string>();

			try
			{
				if(!order.SelfDelivery && order.DeliveryPoint != null && order.DeliveryPoint.Phones.Any())
				{
					var deliveryPointReceiptPhone = order.DeliveryPoint.Phones.FirstOrDefault(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& p.PhoneType?.PhonePurpose == PhonePurpose.ForReceipts
							&& !p.IsArchive);

					if(deliveryPointReceiptPhone != null)
					{
						contacts[2] = "+7" + deliveryPointReceiptPhone.DigitsNumber;
					}

					var phone = order.DeliveryPoint.Phones.FirstOrDefault(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& p.DigitsNumber.Substring(0, 1) == "9"
							&& !p.IsArchive);

					if(phone != null)
					{
						contacts[4] = "+7" + phone.DigitsNumber;
					}
					else if(order.DeliveryPoint.Phones.Any(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& !p.IsArchive))
					{
						contacts[7] = "+7" + order.DeliveryPoint.Phones.FirstOrDefault(
							p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
								&& !p.IsArchive).DigitsNumber;
					}
				}
			}
			catch(GenericADOException ex)
			{
				_logger.LogWarning(ex, "Исключение при попытке поборать телефон для чеков из точки доставки");
			}

			try
			{
				if(order.Client.Phones.Any())
				{
					var clientReceiptPhone = order.Client.Phones.FirstOrDefault(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& p.PhoneType?.PhonePurpose == PhonePurpose.ForReceipts
							&& !p.IsArchive);

					if(clientReceiptPhone != null)
					{
						contacts[3] = "+7" + clientReceiptPhone.DigitsNumber;
					}

					var phone = order.Client.Phones.FirstOrDefault(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& p.DigitsNumber.Substring(0, 1) == "9"
							&& !p.IsArchive);

					if(phone != null)
					{
						contacts[5] = "+7" + phone.DigitsNumber;
					}
					else if(order.Client.Phones.Any(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& !p.IsArchive))
					{
						contacts[8] = "+7" + order.Client.Phones.FirstOrDefault(
							p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
								&& !p.IsArchive).DigitsNumber;
					}
				}
			}
			catch(GenericADOException ex)
			{
				_logger.LogWarning(ex, "Исключение при попытке поборать телефон для чеков из контрагента");
			}

			try
			{
				if(order.Client.Emails.Any())
				{
					var receiptEmail = order.Client.Emails.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Address)
						 && e.EmailType?.EmailPurpose == EmailPurpose.ForReceipts)?.Address;

					if(receiptEmail != null)
					{
						contacts[0] = receiptEmail;
					}

					var billsEmail = order.Client.Emails.FirstOrDefault(
						e => !string.IsNullOrWhiteSpace(e.Address)
							&& e.EmailType?.EmailPurpose == EmailPurpose.ForBills)?.Address;

					if(billsEmail != null)
					{
						contacts[1] = billsEmail;
					}

					var email = order.Client.Emails.FirstOrDefault(e =>
						!string.IsNullOrWhiteSpace(e.Address)
						&& e.EmailType?.EmailPurpose != EmailPurpose.ForBills
						&& e.EmailType?.EmailPurpose != EmailPurpose.ForReceipts)
						?.Address;

					if(email != null)
					{
						contacts[6] = email;
					}
				}
			}
			catch(GenericADOException ex)
			{
				_logger.LogWarning(ex, "Исключение при попытке поборать почту для чеков из контрагента");
			}

			if(!contacts.Any())
			{
				return null;
			}

			var onlyWithValidPhones = contacts.Where(x =>
				(x.Value.StartsWith("+7")
					&& x.Value.Length == 12)
				|| !x.Value.StartsWith("+7"));

			if(!onlyWithValidPhones.Any())
			{
				throw new InvalidOperationException($"Не удалось подобрать контакт для заказа {order.Id}");
			}

			int minWeight = onlyWithValidPhones.Min(c => c.Key);
			var contact = contacts[minWeight];

			if(string.IsNullOrWhiteSpace(contact))
			{
				throw new InvalidOperationException($"Не удалось подобрать контакт для заказа {order.Id}");
			}

			return contact;
		}

	}
}
