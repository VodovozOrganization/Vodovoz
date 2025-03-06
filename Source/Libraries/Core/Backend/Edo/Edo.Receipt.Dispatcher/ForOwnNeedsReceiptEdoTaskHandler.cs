using Core.Infrastructure;
using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems.Custom.Sources;
using Edo.Problems;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using QS.DomainModel.UoW;
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
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Settings.Edo;
using NetTopologySuite.Operation.Valid;
using NHibernate;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.Repositories;

namespace Edo.Receipt.Dispatcher
{
	public class ForOwnNeedsReceiptEdoTaskHandler
	{
		private const int _maxCodesInReceipt = 128;

		private readonly ILogger<ForOwnNeedsReceiptEdoTaskHandler> _logger;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly TransferRequestCreator _transferRequestCreator;
		private readonly IEdoRepository _edoRepository;
		private readonly IEdoReceiptSettings _edoReceiptSettings;
		private readonly TrueMarkTaskCodesValidator _localCodesValidator;
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly Tag1260Checker _tag1260Checker;
		private readonly IGenericRepository<TrueMarkWaterGroupCode> _waterGroupCodeRepository;
		private readonly IGenericRepository<TrueMarkProductCode> _productCodeRepository;
		private readonly IBus _messageBus;
		private readonly IUnitOfWork _uow;
		private readonly EdoTaskValidator _edoTaskValidator;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
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
			IGenericRepository<TrueMarkWaterGroupCode> waterGroupCodeRepository,
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
			_waterGroupCodeRepository = waterGroupCodeRepository ?? throw new ArgumentNullException(nameof(waterGroupCodeRepository));
			_productCodeRepository = productCodeRepository ?? throw new ArgumentNullException(nameof(productCodeRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
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
				await _uow.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
				await _uow.CommitAsync(cancellationToken);
				return;
			}

			// всегда отправлять чек клиенту
			var hasAlwaysSend = receiptEdoTask.OrderEdoRequest.Order.Client.AlwaysSendReceipts;
			if(hasAlwaysSend)
			{
				await PrepareReceipt(receiptEdoTask, trueMarkCodesChecker, cancellationToken);
				await _uow.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
				await _uow.CommitAsync(cancellationToken);
				return;
			}

			// проверка на наличие чека на сумму за сегодня
			var hasReceiptOnSumToday = await HasReceiptOnSumToday(receiptEdoTask, cancellationToken);
			if(!hasReceiptOnSumToday)
			{
				await PrepareReceipt(receiptEdoTask, trueMarkCodesChecker, cancellationToken);
				await _uow.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
				await _uow.CommitAsync(cancellationToken);
				return;
			}

			// сохранение кодов в пул
			// смена статуса задачи на завершен
			// сохранение задачи
			await SaveCodesToPool(receiptEdoTask, cancellationToken);
			receiptEdoTask.Status = EdoTaskStatus.Completed;
			receiptEdoTask.ReceiptStatus = EdoReceiptStatus.SavedToPool;
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

			var sendReceiptMessage = new ReceiptSendEvent { ReceiptEdoTaskId = receiptEdoTask.Id };
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

						// определить что TaskItem принадлежит групповому коду
						if(codeResult.EdoTaskItem.ProductCode.ResultCode.ParentTransportCodeId != null)
						{
							hasGroupInvalidCodes = true;

							// так же надо зачистить все части этого группового кода
							var groupCode = GetParentGroupCode(codeResult.EdoTaskItem.ProductCode.ResultCode.ParentTransportCodeId.Value);
							foreach(var individualCode in groupCode.GetAllCodes().Where(x => x.IsTrueMarkWaterIdentificationCode))
							{
								var foundInvalidGroupIdentificationCode = receiptEdoTask.Items
									.FirstOrDefault(x => x.ProductCode.SourceCode == individualCode.TrueMarkWaterIdentificationCode);
								foundInvalidGroupIdentificationCode.ProductCode.SourceCode = null;
								foundInvalidGroupIdentificationCode.ProductCode.ResultCode = null;
							}
							
							// надо передать информацию о том что код не валиден, и надо переформировать позиции в чеке


							// обнуляем GroupCode, чтобы знать что там был не валидный групповой код,
							// это будет необходимо когда будем обновлять позиции в чеке
							// не групповые позициии всегда имеют ссылку на EdoTaskItem, поэтому путаницы быть не должно
							//foreach(var fiscalDocument in receiptEdoTask.FiscalDocuments)
							//{
							//	var foundGroupPosition = fiscalDocument.InventPositions
							//		.FirstOrDefault(x => x.GroupCode == groupCode);
							//	if(foundGroupPosition != null)
							//	{
							//		foundGroupPosition.GroupCode = null;
							//		break;
							//	}
							//}
						}
						else
						{
							codeResult.EdoTaskItem.ProductCode.ResultCode = null;
						}
					}

					if(hasGroupInvalidCodes)
					{
						// если нашелся групповой код, который не валиден, то лучше полностью переформировать
						// фискальные документы, потому групповой код влияет на кол-во позиций в чеке
						receiptEdoTask.FiscalDocuments.Clear();
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



			// ЭТО ДОЛЖНО БЫТЬ В ТЕСТЕ, УДАЛИТЬ ИЗ ПРОДА
			//var orderSumForReceipt = receiptEdoTask.OrderEdoRequest.Order.OrderItems
			//	.Where(x => x.Count > 0)
			//	.Sum(x => x.Sum);
			//var receiptsSum = receiptEdoTask.FiscalDocuments.SelectMany(x => x.MoneyPositions).Sum(x => x.Sum);
			//if(receiptsSum != orderSumForReceipt)
			//{
			//	throw new InvalidOperationException($"Сумма чека не совпадает с суммой заказа. Сумма заказа: {orderSumForReceipt}, сумма чека: {receiptsSum}");
			//}



			// перевод в отправку
			receiptEdoTask.Status = EdoTaskStatus.InProgress;
			receiptEdoTask.ReceiptStatus = EdoReceiptStatus.Sending;
			receiptEdoTask.StartTime = DateTime.Now;
			receiptEdoTask.CashboxId = receiptEdoTask.OrderEdoRequest.Order.Contract.Organization.CashBoxId;
			await _uow.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			var sendReceiptMessage = new ReceiptSendEvent { ReceiptEdoTaskId = receiptEdoTask.Id };
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
			var receiptEdoTaskProductCodesWithoutConsolidated = receiptEdoTask.Items
				.Select(x => x.ProductCode)
				.Where(x => (x.ResultCode == null || (x.ResultCode.ParentWaterGroupCodeId == null && x.ResultCode.ParentWaterGroupCodeId == null))
					&& (x.SourceCode.ParentWaterGroupCodeId == null && x.SourceCode.ParentWaterGroupCodeId == null))
				.ToList();

			//проверяем продуктовые коды на дубликаты, если дубли найдены, то меняем статус, проблему и устанавливаем кол-во дублей
			CheckProductCodesForDuplicatesAndUpdateIfNeed(receiptEdoTaskProductCodesWithoutConsolidated);

			//создать или обновить немаркированные позиции
			var mainFiscalDocument = UpdateUnmarkedFiscalDocument(receiptEdoTask);

			//создать или обновить маркированные позиции
			var hasMarkedFiscalDocuments = receiptEdoTask.FiscalDocuments.Any(x => x.Index > 0);
			if(hasMarkedFiscalDocuments)
			{
				await UpdateMarkedFiscalDocuments(receiptEdoTask, cancellationToken);
			}
			else
			{
				await CreateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, cancellationToken);
			}

			//создать или обновить сумму в чеках
			foreach(var fiscalDocument in receiptEdoTask.FiscalDocuments)
			{
				UpdateReceiptMoneyPositions(fiscalDocument);
			}
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
			var allCodes = _productCodeRepository
				.Get(_uow, x => x.Id > 0, 0)
				.ToList();

			var codes = _productCodeRepository
				.Get(_uow, x => resultCodeIds.Contains(x.ResultCode.Id), 0)
				.GroupBy(x => x.ResultCode.Id)
				.ToDictionary(x => x.Key, x => x.ToList());

			return codes;
		}

		public EdoFiscalDocument UpdateUnmarkedFiscalDocument(ReceiptEdoTask receiptEdoTask)
		{
			var order = receiptEdoTask.OrderEdoRequest.Order;
			var pricedOrderItems = order.OrderItems
				.Where(x => x.Price != 0m)
				.Where(x => x.Count > 0m);
			var unmarkedOrderItems = pricedOrderItems
				.Where(x => x.Nomenclature.IsAccountableInTrueMark == false);

			var fiscalDocument = receiptEdoTask.FiscalDocuments.FirstOrDefault(x => x.Index == 0);
			if(fiscalDocument == null)
			{
				fiscalDocument = CreateFiscalDocument(receiptEdoTask);
				fiscalDocument.Index = 0;
				receiptEdoTask.FiscalDocuments.Add(fiscalDocument);
			}

			//обновление не маркированных позиций
			foreach(var unmarkedOrderItem in unmarkedOrderItems)
			{
				var inventPosition = fiscalDocument.InventPositions
					.FirstOrDefault(x => x.OrderItem.Id == unmarkedOrderItem.Id);

				if(inventPosition == null)
				{
					inventPosition = CreateInventPosition(unmarkedOrderItem);
					fiscalDocument.InventPositions.Add(inventPosition);
				}

				inventPosition.Quantity = unmarkedOrderItem.Count;
				inventPosition.DiscountSum = unmarkedOrderItem.DiscountMoney;
			}

			return fiscalDocument;
		}

		public async Task CreateMarkedFiscalDocuments(
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
					.Where(x => x.OrderItem.Nomenclature.Gtins.Any(g => g.GtinNumber == groupCode.GTIN));

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

			//foreach(var remainGroupCodeItem in groupCodesWithTaskItems)
			//{
			//	var groupCode = remainGroupCodeItem.Key;
			//	var affectedTaskItems = remainGroupCodeItem.Value;

			//	var individualCodesInGroupCount = affectedTaskItems.Count();

			//	// знаем кол-во кодов в группе
			//	// теперь нужно создать позицию в чек на соответствующее кол-во

			//	// найти товары в заказе подходящие по GTIN группового кода
			//	var availableOrderItems = expandedMarkedItems
			//		.Where(x => x.OrderItem.Nomenclature.Gtins.Any(g => g.GtinNumber == groupCode.GTIN));

			//	if(availableOrderItems.Count() < individualCodesInGroupCount)
			//	{
			//		_logger.LogWarning("Для группового кода Id {groupCodeId} GTIN {groupCodeGTIN} не хватает товаров в заказе.",
			//			groupCode.Id, groupCode.GTIN);
			//		break;
			//	}

			//	var firstsAvailableOrderItem = availableOrderItems.First();
			//	var inventPosition = CreateInventPosition(firstsAvailableOrderItem.OrderItem);
			//	inventPosition.Quantity = 0;

			//	var availableOrderItemsList = availableOrderItems.ToList();
			//	foreach(var availableOrderItem in availableOrderItemsList)
			//	{
			//		// делаем инкремент потомучто expandedOrderItem соответствует одной единице товара в OrderItem
			//		inventPosition.Quantity++;
			//		// добавляем пропроциональную скидку для одной еденицы товара, которая была ранее рассчитана
			//		// при распределении товаров заказа на их кол-во в каждом товаре
			//		inventPosition.DiscountSum += availableOrderItem.DiscountPerSingleItem;
			//		// исключаем обработанный товар из первоначального списка распределенных товаров
			//		// чтобы при обработке следующей группы, этот товар не попал под обработку, потому
			//		// что мы его уже назначили на определенную группу и забрали от него сумму пропорциональной скидки
			//		expandedMarkedItems.Remove(availableOrderItem);

			//		inventPosition.EdoTaskItem = null;
			//		inventPosition.GroupCode = groupCode;
			//		groupFiscalInventPositions.Add(inventPosition);

			//		individualCodesInGroupCount--;

			//		if(individualCodesInGroupCount == 0)
			//		{
			//			break;
			//		}
			//	}
			//}

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

				if(!receiptEdoTask.FiscalDocuments.Contains(currentFiscalDocument))
				{
					receiptEdoTask.FiscalDocuments.Add(currentFiscalDocument);
				}

				// подготавливаем данные для следующей итерации
				documentIndex++;
				currentProcessingGroupPositions = groupFiscalInventPositions
					.Skip(_maxCodesInReceipt * documentIndex)
					.Take(_maxCodesInReceipt);
				currentFiscalDocument = CreateFiscalDocument(receiptEdoTask);
				currentFiscalDocument.Index = documentIndex;
				currentFiscalDocument.DocumentNumber += $"_{documentIndex}";
			} while(groupFiscalInventPositions.Any());


			// ОБРАБОТКА ИНДИВИДУАЛЬНЫХ КОДОВ

			var currentProcessingPositions = expandedMarkedItems
				.Skip(0)
				// выбираем то кол-во позиций которое не хватает до максимального
				// кол-ва позиций в текущем фискальном документе
				.Take(_maxCodesInReceipt - lastGroupFiscalInventPositionsCount);

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
				}

				if(!receiptEdoTask.FiscalDocuments.Contains(currentFiscalDocument))
				{
					receiptEdoTask.FiscalDocuments.Add(currentFiscalDocument);
				}

				// подготавливаем данные для следующей итерации
				documentIndex++;
				currentProcessingPositions = expandedMarkedItems
					.Skip(_maxCodesInReceipt * documentIndex)
					.Take(_maxCodesInReceipt);
				currentFiscalDocument = CreateFiscalDocument(receiptEdoTask);
				currentFiscalDocument.Index = documentIndex;
				currentFiscalDocument.DocumentNumber += $"_{documentIndex}";
			} while(currentProcessingPositions.Any());
		}




		//private class ProductCodesGroup
		//{
		//	public TrueMarkWaterGroupCode GroupCode { get; set; }
		//	public List<TrueMarkWaterIdentificationCode> ChildCodes { get; set; } = new List<TrueMarkWaterIdentificationCode>();
		//}

		private TrueMarkWaterGroupCode GetParentGroupCode(int id)
		{
			var groupCode = _waterGroupCodeRepository
				.Get(_uow, x => x.Id == id, 1)
				.FirstOrDefault();

			if(groupCode.ParentWaterGroupCodeId != null)
			{
				return GetParentGroupCode(groupCode.ParentWaterGroupCodeId.Value);
			}

			return groupCode;
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
				.Select(x => GetParentGroupCode(x.Value))
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


		//private IEnumerable<TrueMarkAnyCode> FindGroupCodes(ReceiptEdoTask receiptEdoTask)
		//{
		//	var result = new List<TrueMarkAnyCode>();

		//	var unprocessedWithResultCode = receiptEdoTask.Items
		//		.Where(x => x.ProductCode.ResultCode != null)
		//		.ToList();

		//	while(unprocessedWithResultCode.Any())
		//	{
		//		var unprocessedItem = unprocessedWithResultCode.First();
		//		if(unprocessedItem.ProductCode.ResultCode.ParentWaterGroupCodeId != null)
		//		{
		//			var groupCode = GetParentGroupCode(unprocessedItem.ProductCode.ResultCode.ParentWaterGroupCodeId.Value);
		//			var allCodes = groupCode.GetAllCodes();
		//			var identificationCodes = allCodes
		//				.Where(x => x.IsTrueMarkWaterIdentificationCode)
		//				.Select(x => x.TrueMarkWaterIdentificationCode);

		//			foreach(var identificationCode in identificationCodes)
		//			{
		//				var taskItemToRemove = unprocessedWithResultCode.FirstOrDefault(x => x.ProductCode.ResultCode.Id == identificationCode.Id);
		//				unprocessedWithResultCode.Remove(taskItemToRemove);
		//			}

		//			result.Add(groupCode);
		//		}
		//		else
		//		{
		//			unprocessedWithResultCode.Remove(unprocessedItem);
		//			result.Add(unprocessedItem.ProductCode.ResultCode);
		//		}
		//	}

		//	// добавить обработку остальных кодов у которых нет ResultCode
			
		//	return result;
		//}










		private void UpdateReceiptMoneyPositions(EdoFiscalDocument currentFiscalDocument)
		{
			var receiptSum = currentFiscalDocument.InventPositions
				.Sum(x =>  x.Price * x.Quantity - x.DiscountSum);

			var moneyPosition = new FiscalMoneyPosition
			{
				PaymentType = FiscalPaymentType.Cash,
				Sum = receiptSum
			};

			currentFiscalDocument.MoneyPositions.Clear();
			currentFiscalDocument.MoneyPositions.Add(moneyPosition);
		}

		private async Task UpdateMarkedFiscalDocuments(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			// обновляем существующие документы
			// тут не может появится новых orderItems
			// поэтому просто проверяем коды в существующих документах
			// и обновляем их если необходимо

			foreach(var fiscalDocument in receiptEdoTask.FiscalDocuments)
			{
				// в первую очередь надо найти групповые позиции которым нужно обновить коды
				// чтобы обновить групповой код, нужно позицию разбить на индивидуальные позиции
				// и заполниь их кодами из пула, при этом создав новые EdoTaskItem

				//var markedEmptyGroupPositions = fiscalDocument.InventPositions
				//	.Where(x => x.OrderItem.Nomenclature.IsAccountableInTrueMark)
				//	// Если EdoTaskItem == null, значит это групповая позиция
				//	.Where(x => x.EdoTaskItem == null)
				//	// Если GroupCode == null, значит что групповой код был не валиден в ЧЗ
				//	.Where(x => x.GroupCode == null)
				//	;

				//foreach(var emptyGroupPosition in markedEmptyGroupPositions)
				//{
				//	fiscalDocument.InventPositions.Remove(emptyGroupPosition);

				//	receiptEdoTask.
				//	emptyGroupPosition.GroupCode

				//}


				// потом обновить по старому все остальные :

				var markedEmptyPositions = fiscalDocument.InventPositions
					.Where(x => x.OrderItem.Nomenclature.IsAccountableInTrueMark)
					.Where(x => x.EdoTaskItem.ProductCode.ResultCode == null)
					;

				foreach(var inventPosition in markedEmptyPositions)
				{
					var code = await LoadCodeFromPool(inventPosition.OrderItem.Nomenclature, cancellationToken);
					inventPosition.EdoTaskItem.ProductCode.ResultCode = code;
				}
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
			inventPosition.OrderItem = orderItem;

			// Пытаемся найти совпадающий по Gtin код:

			// сначала у кого заполнен Result код
			//var resultCodes = unprocessedCodes
			//	.Where(x => x.ProductCode.Problem == ProductCodeProblem.None)
			//	.Where(x => x.ProductCode.ResultCode != null);
			//foreach(var gtin in orderItem.Nomenclature.Gtins)
			//{
			//	matchEdoTaskItem = resultCodes
			//		.Where(x => x.ProductCode.ResultCode.GTIN == gtin.GtinNumber)
			//		.FirstOrDefault();
			//	if(matchEdoTaskItem != null)
			//	{
			//		inventPosition.EdoTaskItem = matchEdoTaskItem;
			//		unprocessedCodes.Remove(matchEdoTaskItem);
			//		return inventPosition;
			//	}
			//}

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
					codeIdFromPool = await _trueMarkCodesPool.TakeCode(gtin.GtinNumber, cancellationToken);
					matchEdoTaskItem.ProductCode.ResultCode = new TrueMarkWaterIdentificationCode { Id = codeIdFromPool };
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
					codeIdFromPool = await _trueMarkCodesPool.TakeCode(gtin.GtinNumber, cancellationToken);
					matchEdoTaskItem.ProductCode.ResultCode = new TrueMarkWaterIdentificationCode { Id = codeIdFromPool };
					matchEdoTaskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;
					await _uow.SaveAsync(matchEdoTaskItem, cancellationToken: cancellationToken);

					inventPosition.EdoTaskItem = matchEdoTaskItem;
					unprocessedCodes.Remove(matchEdoTaskItem);
					return inventPosition;
				}
			}

			// Если ничего не смогли подобрать то создаем новую запись
			var code = await LoadCodeFromPool(orderItem.Nomenclature, cancellationToken);
			var newProductCode = new AutoTrueMarkProductCode
			{
				Problem = ProductCodeProblem.Unscanned,
				CustomerEdoRequest = receiptEdoTask.OrderEdoRequest,
				SourceCodeStatus = SourceProductCodeStatus.Changed,
				SourceCode = null,
				ResultCode = code
			};

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
			foreach(var gtin in nomenclature.Gtins.Reverse())
			{
				try
				{
					codeId = await _trueMarkCodesPool.TakeCode(gtin.GtinNumber, cancellationToken);
				}
				catch(EdoCodePoolException)
				{
					continue;
				}
			}
			if(codeId == 0)
			{
				throw new EdoCodePoolException($"В пуле не найдено кодов для номенклатуры {nomenclature}");
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

			bool isValid = true;
			var invalidTaskItems = new List<EdoTaskItem>();

			foreach(var fiscalDocument in receiptEdoTask.FiscalDocuments)
			{
				var codesToCheck1260 = fiscalDocument.InventPositions
					.Where(x => x.EdoTaskItem.ProductCode.ResultCode != null)
					.ToDictionary(x => x.EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260);

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
					var taskItems = invalidCodes.Select(x => codesToCheck1260[x.Cis].EdoTaskItem);
					invalidTaskItems.AddRange(taskItems);
					isValid = false;
					continue;
				}

				foreach(var codeResult in result.Codes)
				{
					var inventPosition = codesToCheck1260[codeResult.Cis];
					inventPosition.IndustryRequisiteData = $"UUID={result.ReqId}&Time={result.ReqTimestamp}";
				}
			}

			if(isValid)
			{
				_edoProblemRegistrar.SolveCustomProblem<IndustryRequisiteMissingOrganizationToken>(receiptEdoTask);
				_edoProblemRegistrar.SolveCustomProblem<IndustryRequisiteCheckApiError>(receiptEdoTask);
				return IndustryRequisitePrepareResult.Succeeded;
			}
			else
			{
				return IndustryRequisitePrepareResult.NeedToChange;
			}
		}

		//private async Task<bool> PrepareIndustryRequisite(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		//{
		//	var seller = receiptEdoTask.OrderEdoRequest.Order.Contract.Organization;
		//	var sellerEdoKey = seller.EdoKey;
		//	if(sellerEdoKey == null)
		//	{
		//		throw new InvalidOperationException($"Для организации {seller.Id} не установлен ключ для доступа к ЭДО");
		//	}

		//	bool isValid = true;
		//	foreach(var fiscalDocument in receiptEdoTask.FiscalDocuments)
		//	{
		//		var codesToCheck1260 = fiscalDocument.InventPositions
		//			.Where(x => x.EdoTaskItem.ProductCode.ResultCode != null)
		//			.ToDictionary(x => x.EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260);

		//		var result = await _tag1260Checker.CheckCodesForTag1260Async(
		//			codesToCheck1260.Keys, 
		//			sellerEdoKey.Value, 
		//			cancellationToken
		//		);

		//		if(result.Code != 0)
		//		{
		//			throw new InvalidOperationException($"Ошибка при итоговой проверке кодов в ЧЗ. Код ошибки: {result.Code}, сообщение: {result.Description}");
		//		}

		//		foreach(var codeResult in result.Codes)
		//		{
		//			var canSell = codeResult.ErrorCode == 0
		//				&& codeResult.Found
		//				&& codeResult.Valid
		//				&& codeResult.Verified
		//				&& codeResult.ExpireDate > DateTime.Now
		//				&& codeResult.Realizable
		//				&& codeResult.Utilised
		//				&& !codeResult.IsBlocked
		//				&& !codeResult.Sold;

		//			var inventPosition = codesToCheck1260[codeResult.Cis];
		//			if(!canSell)
		//			{
		//				isValid = false;
		//				inventPosition.EdoTaskItem.ProductCode.ResultCode = null;
		//			}
		//			else
		//			{
		//				inventPosition.IndustryRequisiteData = $"UUID={result.ReqId}&Time={result.ReqTimestamp}";
		//			}
		//		}
		//	}

		//	return isValid;
		//}


		//private async Task CreateFiscalDocuments(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		//{
		//	var regulatoryDocumentId = _edoReceiptSettings.IndustryRequisiteRegulatoryDocumentId;
		//	var regulatoryDocument = await _uow.Session
		//		.GetAsync<FiscalIndustryRequisiteRegulatoryDocument>(regulatoryDocumentId, cancellationToken);
		//	if(regulatoryDocument == null)
		//	{
		//		throw new InvalidOperationException($"Не найден регламентирующий документ " +
		//			$"{nameof(FiscalIndustryRequisiteRegulatoryDocument)} с id {regulatoryDocumentId}," +
		//			$" необходимый для настройки разрешительного режима");
		//	}




		//	var order = receiptEdoTask.OrderEdoRequest.Order;
		//	var pricedOrderItems = order.OrderItems
		//		.Where(x => x.Price != 0m)
		//		.Where(x => x.Count > 0m);
		//	var notMarkedOrderItems = pricedOrderItems
		//		.Where(x => x.Nomenclature.IsAccountableInTrueMark == false);
		//	var markedOrderItems = pricedOrderItems
		//		.Where(x => x.Nomenclature.IsAccountableInTrueMark == true);

		//	var mainFiscalDocument = CreateFiscalDocument(receiptEdoTask, order);

		//	//создать не маркированные позиции
		//	foreach(var notMarkedOrderItem in notMarkedOrderItems)
		//	{
		//		var inventPosition = CreateInventPosition(notMarkedOrderItem);
		//		inventPosition.Quantity = notMarkedOrderItem.Count;
		//		mainFiscalDocument.InventPositions.Add(inventPosition);
		//	}

		//	//создать маркированные позиции

		//	// умножает каждую позицию на кол-во товаров, возвращает в едином списке
		//	var totalCountedMarkedItems = markedOrderItems.SelectMany(orderItem => 
		//	{
		//		var multipliedItems = new List<(OrderItemEntity OrderItem, decimal DiscountPerSingleItem)>();

		//		decimal wholeDiscount = 0;
		//		//i == 1 чтобы пропуcтить последний элемент, у него расчет происходит из остатков
		//		for(int i = 1; i < orderItem.Count; i++)
		//		{
		//			var itemDiscount = 0m;
		//			if(wholeDiscount < orderItem.DiscountMoney)
		//			{
		//				var partDiscount = Math.Round(orderItem.DiscountMoney / orderItem.Count, 1);
		//				wholeDiscount += partDiscount;
		//				itemDiscount = partDiscount;
		//			}
		//			multipliedItems.Add((orderItem, itemDiscount));
		//		}

		//		//добавление последнего элемента с остатками от целой скидки
		//		var residueDiscount = orderItem.DiscountMoney - wholeDiscount;
		//		if(residueDiscount < 0)
		//		{
		//			residueDiscount = 0;
		//		}
		//		multipliedItems.Add((orderItem, residueDiscount));

		//		return multipliedItems;
		//	});

		//	var codes = receiptEdoTask.Items.Select(x => x.ProductCode.ResultCode).ToList();


		//	var currentFiscalDocument = mainFiscalDocument;
		//	var currentProcessingPositions = totalCountedMarkedItems.Skip(0).Take(_maxCodesInReceipt);
		//	do
		//	{
		//		foreach(var item in currentProcessingPositions)
		//		{
		//			var markedOrderItem = item.OrderItem;
		//			var discount = item.DiscountPerSingleItem;
		//			var inventPosition = CreateInventPosition(markedOrderItem);
		//			inventPosition.Quantity = 1;
		//			inventPosition.DiscountSum = discount;

		//			// маркировка
		//			// поиск кода по Gtin
		//			foreach(var gtin in markedOrderItem.Nomenclature.Gtins)
		//			{
		//				var matchCode = codes.FirstOrDefault(x => x.GTIN == gtin.GtinNumber);
		//				if(matchCode != null)
		//				{
		//					codes.Remove(matchCode);
		//					inventPosition.ProductMark = matchCode.FullCode;
		//					break;
		//				}
		//			}

		//			currentFiscalDocument.InventPositions.Add(inventPosition);

		//			var sum = Math.Round(markedOrderItem.Price - discount, 2);
		//			receiptSummary += sum;
		//		}

		//		receiptEdoTask.FiscalDocuments.Add(currentFiscalDocument);

		//		var nextInnerNumber = receiptEdoTask.FiscalDocuments.Count;
		//		currentProcessingPositions = totalCountedMarkedItems
		//			.Skip(_maxCodesInReceipt * nextInnerNumber)
		//			.Take(_maxCodesInReceipt);
		//		currentFiscalDocument = CreateFiscalDocument(receiptEdoTask, order);
		//		currentFiscalDocument.DocumentNumber += $"_{nextInnerNumber}";
		//	} while(currentProcessingPositions.Any());





		//	var remainingMarkedOrderItems = markedOrderItems.Skip(_maxCodesInReceipt);















		//	foreach(var taskItem in receiptEdoTask.Items)
		//	{
		//		if(taskItem.ProductCode.ResultCode == null)
		//		{
		//			taskItem.
		//			_trueMarkCodesPool.TakeCode();
		//			taskItem.ProductCode.ResultCode =
		//		}



		//		var code = taskItem.ProductCode;

		//		taskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;

		//		if(taskItem.ProductCode.Problem != ProductCodeProblem.None && )
		//		{
		//			var code = _uow.GetById<TrueMarkWaterIdentificationCode>(codeId);
		//			taskItem.
		//		}
		//	}


		//	_trueMarkCodesPool.TakeCode();
		//	// проверка и замена кодов из пула

		//	// проверка в ЧЗ, если не валидны то назад
		//}

		//private void CreateIndustryRequisite(InventPosition inventPosition, TrueMarkWaterIdentificationCode code)
		//{
		//	inventPosition.IndustryRequisite = new IndustryRequisite
		//	{
		//		inventPosition.RegulatoryDocument = regulatoryDocument;
		//		DocData = $"UUID={code.Tag1260CodeCheckResult.ReqId}&Time={code.Tag1260CodeCheckResult.ReqTimestamp}"
		//	};
		//}

		private EdoFiscalDocument CreateFiscalDocument(ReceiptEdoTask receiptEdoTask)
		{
			var order = receiptEdoTask.OrderEdoRequest.Order;
			var mainFiscalDocument = new EdoFiscalDocument
			{
				ReceiptEdoTask = receiptEdoTask,
				Stage = FiscalDocumentStage.Preparing,
				Status = FiscalDocumentStatus.None,
				DocumentGuid = Guid.NewGuid(),
				DocumentNumber = $"vod_{order.Id}",
				DocumentType = FiscalDocumentType.Sale,
				CheckoutTime = order.TimeDelivered ?? DateTime.Now,
				Contact = GetContact(order),
				//Для собственных нужд не заполняется
				ClientInn = null,
				CashierName = order.Contract?.Organization?.ActiveOrganizationVersion?.Leader?.ShortName,
				//По умолчанию не печатаем чеки
				PrintReceipt = false
			};
			return mainFiscalDocument;
		}

		private FiscalInventPosition CreateInventPosition(OrderItemEntity orderItem)
		{
			var inventPosition = new FiscalInventPosition
			{
				Name = orderItem.Nomenclature.OfficialName,
				Price = Math.Round(orderItem.Price, 2),
				OrderItem = orderItem
			};

			var organization = orderItem.Order.Contract?.Organization;

			if(organization is null || organization.WithoutVAT || orderItem.Nomenclature.VAT == VAT.No)
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



	//public class ResaleReceiptEdoTaskHandler
	//{
	//	private readonly IUnitOfWorkFactory _uowFactory;
	//	private readonly EdoTaskMainValidator _validator;
	//	private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
	//	private readonly TransferRequestCreator _transferRequestCreator;
	//	private readonly IBus _messageBus;
	//	private readonly IUnitOfWork _uow;

	//	public ResaleReceiptEdoTaskHandler(
	//		IUnitOfWorkFactory uowFactory,
	//		EdoTaskMainValidator validator,
	//		EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
	//		TransferRequestCreator transferRequestCreator,
	//		IBus messageBus
	//		)
	//	{
	//		_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
	//		_validator = validator ?? throw new ArgumentNullException(nameof(validator));
	//		_edoTaskTrueMarkCodeCheckerFactory = edoTaskTrueMarkCodeCheckerFactory ?? throw new ArgumentNullException(nameof(edoTaskTrueMarkCodeCheckerFactory));
	//		_transferRequestCreator = transferRequestCreator ?? throw new ArgumentNullException(nameof(transferRequestCreator));
	//		_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
	//		_uow = uowFactory.CreateWithoutRoot();
	//	}

	//	// handle new
	//	// Entry stage: New
	//	// Validated stage: New
	//	// Changed to: Transfering, Sending
	//	// [событие от scheduler]
	//	// (проверяет нужен ли перенос, или сразу отправляет)
	//	public async Task HandleResaleReceipt(int receiptEdoTaskId, CancellationToken cancellationToken)
	//	{
	//		var edoTask = await _uow.Session.GetAsync<ReceiptEdoTask>(receiptEdoTaskId, cancellationToken);
	//		// TEST
	//		// проверяем все коды как МН
	//		var trueMarkApiClient = new TrueMarkApiClient("https://test-mn-truemarkapi.dev.vod.qsolution.ru/", "test");
	//		var trueMarkCodeChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask, trueMarkApiClient);

	//		var valid = await Validate(edoTask, trueMarkCodeChecker, cancellationToken);
	//		if(!valid)
	//		{
	//			await _uow.CommitAsync(cancellationToken);
	//			return;
	//		}

	//		object message = null;

	//		// Определя
	//	}
	//}
}
