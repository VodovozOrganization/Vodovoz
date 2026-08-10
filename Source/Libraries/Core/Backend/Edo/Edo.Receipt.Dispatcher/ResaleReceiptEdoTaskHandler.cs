using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Exception.EdoExceptions;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Admin;
using TrueMark.Library;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Client;
using Vodovoz.Settings.Edo;
using Vodovoz.Settings.Organizations;

namespace Edo.Receipt.Dispatcher
{
	public class ResaleReceiptEdoTaskHandler : IDisposable
	{
		private readonly ILogger<ResaleReceiptEdoTaskHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly EdoTaskValidator _edoTaskValidator;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly TransferRequestCreator _transferRequestCreator;
		private readonly ITrueMarkCodesValidator _localCodesValidator;
		private readonly ITrueMarkCodesValidator _trueMarkTaskCodesValidator;
		private readonly Tag1260Checker _tag1260Checker;
		private readonly IEdoReceiptSettings _edoReceiptSettings;
		private readonly IEdoOrderContactProvider _edoOrderContactProvider;
		private readonly ISaveCodesService _saveCodesService;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly IBus _messageBus;
		private readonly EdoCancellationService _edoCancellationService;
		private readonly int _maxCodesInReceipt;

		public ResaleReceiptEdoTaskHandler(
			ILogger<ResaleReceiptEdoTaskHandler> logger,
			IUnitOfWork uow,
			EdoTaskValidator edoTaskValidator,
			EdoProblemRegistrar edoProblemRegistrar,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			TransferRequestCreator transferRequestCreator,
			ITrueMarkCodesValidator localCodesValidator,
			ITrueMarkCodesValidator trueMarkTaskCodesValidator,
			Tag1260Checker tag1260Checker,
			IEdoReceiptSettings edoReceiptSettings,
			IEdoOrderContactProvider edoOrderContactProvider,
			ISaveCodesService saveCodesService,
			IOrganizationSettings organizationSettings,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			IBus messageBus,
			EdoCancellationService edoCancellationService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_edoTaskValidator = edoTaskValidator ?? throw new ArgumentNullException(nameof(edoTaskValidator));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_edoTaskTrueMarkCodeCheckerFactory = edoTaskTrueMarkCodeCheckerFactory ?? throw new ArgumentNullException(nameof(edoTaskTrueMarkCodeCheckerFactory));
			_transferRequestCreator = transferRequestCreator ?? throw new ArgumentNullException(nameof(transferRequestCreator));
			_localCodesValidator = localCodesValidator ?? throw new ArgumentNullException(nameof(localCodesValidator));
			_trueMarkTaskCodesValidator = trueMarkTaskCodesValidator ?? throw new ArgumentNullException(nameof(trueMarkTaskCodesValidator));
			_tag1260Checker = tag1260Checker ?? throw new ArgumentNullException(nameof(tag1260Checker));
			_edoReceiptSettings = edoReceiptSettings ?? throw new ArgumentNullException(nameof(edoReceiptSettings));
			_edoOrderContactProvider = edoOrderContactProvider ?? throw new ArgumentNullException(nameof(edoOrderContactProvider));
			_saveCodesService = saveCodesService ?? throw new ArgumentNullException(nameof(saveCodesService));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_edoCancellationService = edoCancellationService ?? throw new ArgumentNullException(nameof(edoCancellationService));

			_maxCodesInReceipt = _edoReceiptSettings.MaxCodesInReceiptCount;
		}

		public async Task HandleNewReceipt(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			var order = receiptEdoTask.FormalEdoRequest.Order;
			if(order.Client.ReasonForLeaving != ReasonForLeaving.Resale)
			{
				throw new InvalidOperationException($"Попытка обработать чек с причиной выбытия " +
					$"{order.Client.ReasonForLeaving} обработчиком для {ReasonForLeaving.Resale}.");
			}

			// предзагрузка для ускорения
			var productCodes = await _uow.Session.QueryOver<TrueMarkProductCode>()
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.Where(x => x.CustomerEdoRequest.Id == receiptEdoTask.FormalEdoRequest.Id)
				.ListAsync();

			var taskCodes = await _uow.Session.QueryOver<EdoTaskItem>()
				.Fetch(SelectMode.Fetch, x => x.ProductCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.ResultCode.Tag1260CodeCheckResult)
				.Where(x => x.CustomerEdoTask.Id == receiptEdoTask.Id)
				.ListAsync(cancellationToken);

			var totalProductCodes = productCodes
				.Union(taskCodes.Select(x => x.ProductCode));

			var sourceCodes = totalProductCodes
				.Where(x => x.SourceCode != null)
				.Select(x => x.SourceCode);

			var resultCodes = totalProductCodes
				.Where(x => x.ResultCode != null)
				.Select(x => x.ResultCode);

			var codesToPreload = sourceCodes.Union(resultCodes).Distinct();
			await _trueMarkCodeRepository.PreloadCodes(codesToPreload, cancellationToken);

			if(productCodes.Any(x => x.SourceCodeStatus == SourceProductCodeStatus.Rejected))
			{
				_logger.LogInformation("Задача Id {DocumentEdoTaskId} имеет отклоненные коды, " +
					"значит отправка будет производиться другой задачей", receiptEdoTask.Id);
				return;
			}

			if(_edoCancellationService.IsEdoTaskMustBeCancelled(receiptEdoTask))
			{
				var reason = "Проблема с составом заказа. Сумма заказа или одна из позиций заказа меньше нуля";

				await _edoCancellationService.CancelTask(receiptEdoTask.Id, reason, false, cancellationToken);
				return;
			}

			var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(receiptEdoTask);

			var isValid = await _edoTaskValidator.Validate(receiptEdoTask, cancellationToken, trueMarkCodesChecker);
			if(!isValid)
			{
				return;
			}

			await PrepareFiscalDocuments(receiptEdoTask, cancellationToken);

			// проверяем все коды по задаче в ЧЗ
			var taskValidationResult = await _trueMarkTaskCodesValidator.ValidateAsync(
				receiptEdoTask,
				trueMarkCodesChecker,
				cancellationToken
			);

			if(!taskValidationResult.IsAllValid)
			{
				// Регистрация проблемы и выход
				return;
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

				TryRecalculateOrderVat(receiptEdoTask);

				await _uow.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
				await _uow.CommitAsync(cancellationToken);

				var receiptTransferMessage = new TransferRequestCreatedEvent { TransferIterationId = iteration.Id };
				await _messageBus.Publish(receiptTransferMessage);
				return;
			}

			// итоговая валидация и получение разрешительного режима
			var industryRequisitePrepared = await PrepareIndustryRequisite(receiptEdoTask, cancellationToken);
			if(!industryRequisitePrepared)
			{
				return;
			}

			// перевод в отправку
			receiptEdoTask.Status = EdoTaskStatus.InProgress;
			receiptEdoTask.ReceiptStatus = EdoReceiptStatus.Sending;
			receiptEdoTask.StartTime = DateTime.Now;
			receiptEdoTask.CashboxId = receiptEdoTask.FormalEdoRequest.Order.Contract.Organization.CashBoxId;

			TryRecalculateOrderVat(receiptEdoTask);

			await _uow.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			var sendReceiptMessage = new ReceiptReadyToSendEvent { ReceiptEdoTaskId = receiptEdoTask.Id };
			await _messageBus.Publish(sendReceiptMessage);
		}

		public async Task HandleTransferComplete(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			// предзагрузка для ускорения
			var productCodes = await _uow.Session.QueryOver<TrueMarkProductCode>()
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.Where(x => x.CustomerEdoRequest.Id == receiptEdoTask.FormalEdoRequest.Id)
				.ListAsync();

			var taskCodes = await _uow.Session.QueryOver<EdoTaskItem>()
				.Fetch(SelectMode.Fetch, x => x.ProductCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.ResultCode.Tag1260CodeCheckResult)
				.Where(x => x.CustomerEdoTask.Id == receiptEdoTask.Id)
				.ListAsync(cancellationToken);

			var totalProductCodes = productCodes
				.Union(taskCodes.Select(x => x.ProductCode));

			var sourceCodes = totalProductCodes
				.Where(x => x.SourceCode != null)
				.Select(x => x.SourceCode);

			var resultCodes = totalProductCodes
				.Where(x => x.ResultCode != null)
				.Select(x => x.ResultCode);

			var codesToPreload = sourceCodes.Union(resultCodes).Distinct();
			await _trueMarkCodeRepository.PreloadCodes(codesToPreload, cancellationToken);

			if(productCodes.Any(x => x.SourceCodeStatus == SourceProductCodeStatus.Rejected))
			{
				_logger.LogInformation("Задача Id {DocumentEdoTaskId} имеет отклоненные коды, " +
					"значит отправка будет производиться другой задачей", receiptEdoTask.Id);
				return;
			}

			if(_edoCancellationService.IsEdoTaskMustBeCancelled(receiptEdoTask))
			{
				var reason = "Проблема с составом заказа. Сумма заказа или одна из позиций заказа меньше нуля";

				await _edoCancellationService.CancelTask(receiptEdoTask.Id, reason, false, cancellationToken);
				return;
			}

			var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(receiptEdoTask);
			var isValid = await _edoTaskValidator.Validate(receiptEdoTask, cancellationToken, trueMarkCodesChecker);
			if(!isValid)
			{
				return;
			}

			var taskValidationResult = await _localCodesValidator.ValidateAsync(
				receiptEdoTask,
				trueMarkCodesChecker,
				cancellationToken
			);

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
			var industryRequisitePrepared = await PrepareIndustryRequisite(receiptEdoTask, cancellationToken);
			if(!industryRequisitePrepared)
			{
				return;
			}

			// перевод в отправку
			receiptEdoTask.Status = EdoTaskStatus.InProgress;
			receiptEdoTask.ReceiptStatus = EdoReceiptStatus.Sending;
			receiptEdoTask.StartTime = DateTime.Now;
			receiptEdoTask.CashboxId = receiptEdoTask.FormalEdoRequest.Order.Contract.Organization.CashBoxId;
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
			//создать или обновить немаркированные позиции
			var mainFiscalDocument = UpdateUnmarkedFiscalDocument(receiptEdoTask);

			//создать или обновить маркированные позиции
			await UpdateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, cancellationToken);

			//создать или обновить сумму в чеках
			foreach(var fiscalDocument in receiptEdoTask.FiscalDocuments)
			{
				UpdateReceiptMoneyPositions(fiscalDocument);
			}
		}

		private EdoFiscalDocument UpdateUnmarkedFiscalDocument(ReceiptEdoTask receiptEdoTask)
		{
			var order = receiptEdoTask.FormalEdoRequest.Order;
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

		private async Task UpdateMarkedFiscalDocuments(
			ReceiptEdoTask receiptEdoTask,
			EdoFiscalDocument mainFiscalDocument,
			CancellationToken cancellationToken
			)
		{
			var order = receiptEdoTask.FormalEdoRequest.Order;
			var markedOrderItems = order.OrderItems
				.Where(x => x.Price != 0m)
				.Where(x => x.Count > 0m)
				.Where(x => x.Nomenclature.IsAccountableInTrueMark == true);

			var expandedMarkedItems = ExpandMarkedOrderItems(markedOrderItems).ToList();
			var unprocessedCodes = receiptEdoTask.Items.ToList();


			// ОБРАБОТКА ГРУППОВЫХ КОДОВ

			// отобрали от списка необработанных кодов все групповые коды
			// их обработаем в первую очередь
			var groupCodesWithTaskItems = await TakeGroupCodesWithTaskItems(unprocessedCodes, cancellationToken);

			var groupFiscalInventPositions = new List<FiscalInventPosition>();

			foreach(var groupCodeWithTaskItems in groupCodesWithTaskItems.ToList())
			{
				var groupCode = groupCodeWithTaskItems.Key;
				var affectedTaskItems = groupCodeWithTaskItems.Value;

				var isGroupCodeUsedInEdoDocument = _trueMarkCodeRepository.IsGroupCodeUsedInEdoDocument(groupCode.Id);
				if(isGroupCodeUsedInEdoDocument)
				{
					// Уже используется в каком-то ЭДО документе, пропускаем
					groupCodesWithTaskItems.Remove(groupCode);
					continue;
				}

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

					// проверяем, что стоимость за единицу товара для группового кода можно
					// получить без остатка (до копеек). Если нет, то не объединяем строки заказа под
					// групповой код на этом OrderItem, продолжаем искать другой подходящий OrderItem
					var pricePerItemForGroupCode = Math.Round(orderItem.Price, 2);
					var groupCodeNetSum =
						pricePerItemForGroupCode * individualCodesInGroupCount
						- expandedOrderItemsForOrderItemList
							.Take(individualCodesInGroupCount)
							.Sum(x => x.DiscountPerSingleItem);

					if(!CanSplitGroupEvenly(groupCodeNetSum, individualCodesInGroupCount))
					{
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
					foreach(var taskItem in affectedTaskItems)
					{
						if(taskItem.ProductCode.ResultCode == null)
						{
							taskItem.ProductCode.ResultCode = taskItem.ProductCode.SourceCode;
							taskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Accepted;
						}
					}

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

			foreach(var remainGroupCodeItem in groupCodesWithTaskItems.ToList())
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
					throw new ResaleMissingCodesException(
						$"Для группового кода Id {groupCode.Id} GTIN {groupCode.GTIN} не хватает товаров в заказе " +
						$"(нужно {individualCodesInGroupCount}, доступно {orderItemsForInventoryPosition.Count}). " +
						"Групповой код нельзя отправить на фискализацию.");
				}

				var orderItemsForInventoryPositionPricesSum =
					orderItemsForInventoryPosition.Sum(x => x.OrderItem.Price);

				var orderItemsForInventoryPositionDiscountsSum =
					orderItemsForInventoryPosition.Sum(x => x.DiscountPerSingleItem);

				// проверяем, что чистую стоимость за единицу товара для группового кода можно
				// получить без остатка (до копеек). Если нет, то не объединяем строки заказа под
				// групповой код
				var groupCodeNetSum =
					orderItemsForInventoryPositionPricesSum - orderItemsForInventoryPositionDiscountsSum;

				if(!CanSplitGroupEvenly(groupCodeNetSum, individualCodesInGroupCount))
				{
					throw new ResaleMissingCodesException(
						$"Для группового кода Id {groupCode.Id} GTIN {groupCode.GTIN} нельзя сформировать строку чека: " +
						"чистая сумма не делится на количество без остатка до копеек. " +
						"Групповой код нельзя заменить индивидуальными (нет криптохвоста).");
				}

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

				// группа успешно назначена в чек — убираем её из словаря
				groupCodesWithTaskItems.Remove(groupCode);

				// принимаем Source как Result у кодов, входящих в группу:
				// групповой код уже уйдёт на фискализацию, а task items должны
				// считаться Accepted
				foreach(var taskItem in remainGroupCodeItem.Value)
				{
					if(taskItem.ProductCode.ResultCode == null)
					{
						taskItem.ProductCode.ResultCode = taskItem.ProductCode.SourceCode;
						taskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Accepted;
					}
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
						.Skip(_maxCodesInReceipt * (documentIndex + 1))
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
					var inventPosition = PrepareMarkedInventPosition(
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

			// Очистка неиспользованных кодов (в пул не сохраняем — только для перепродажи)
			foreach(var unprocessedCode in unprocessedCodes)
			{
				receiptEdoTask.Items.Remove(unprocessedCode);
				await _uow.DeleteAsync(unprocessedCode, cancellationToken);
			}

			// Удаление из задачи не используемых групповых кодов
			foreach(var groupCodeWithTaskItems in groupCodesWithTaskItems)
			{
				foreach(var groupCodeTaskItem in groupCodeWithTaskItems.Value)
				{
					receiptEdoTask.Items.Remove(groupCodeTaskItem);
					await _uow.DeleteAsync(groupCodeTaskItem, cancellationToken);
				}
			}
		}

		/// <summary>
		/// Определяет, можно ли получить чистую стоимость за единицу товара для группового кода
		/// без остатка (с точностью до копеек). Если нет, то товары нельзя объединять в одну
		/// строку чека под этот групповой код
		/// </summary>
		/// <param name="netSum">Чистая стоимость строки чека (цена * кол-во - скидка)</param>
		/// <param name="quantity">Количество товаров, покрываемых групповым кодом</param>
		private static bool CanSplitGroupEvenly(decimal netSum, decimal quantity)
		{
			if(quantity <= 0)
			{
				return false;
			}

			return (netSum * 100m) % quantity == 0;
		}

		private async Task<IDictionary<TrueMarkWaterGroupCode, IEnumerable<EdoTaskItem>>> TakeGroupCodesWithTaskItems(
			List<EdoTaskItem> unprocessedTaskItems,
			CancellationToken cancellationToken
			)
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

			var parentCodes = new List<TrueMarkWaterGroupCode>();
			foreach(var parentCodesId in parentCodesIds)
			{
				var parentCode = await _trueMarkCodeRepository.GetGroupCode(parentCodesId.Value, cancellationToken);

				if(parentCode == null)
				{
					continue;
				}

				parentCodes.Add(parentCode);
			}

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
			var order = currentFiscalDocument.ReceiptEdoTask.FormalEdoRequest.Order;

			var receiptSum = currentFiscalDocument.InventPositions
				.Sum(x => x.Price * x.Quantity - x.DiscountSum);

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
		/// </summary>
		/// <param name="unprocessedCodes">Список всех отсканированных кодов еще необработанных данным методом,
		/// после подбора код исключается из него</param>
		private FiscalInventPosition PrepareMarkedInventPosition(
			ReceiptEdoTask receiptEdoTask,
			OrderItemEntity orderItem,
			List<EdoTaskItem> unprocessedCodes,
			CancellationToken cancellationToken
			)
		{
			var inventPosition = CreateInventPosition(orderItem);
			inventPosition.Quantity = 1;

			// сначала у кого заполнен Result код
			var resultCodes = unprocessedCodes
				.Where(x => x.ProductCode.Problem == ProductCodeProblem.None)
				.Where(x => x.ProductCode.ResultCode != null)
				.Where(x => x.ProductCode.ResultCode.CheckCode != null);

			foreach(var gtin in orderItem.Nomenclature.Gtins)
			{
				var matchEdoTaskItem = resultCodes
					.Where(x => x.ProductCode.ResultCode.Gtin == gtin.GtinNumber)
					.FirstOrDefault();
				if(matchEdoTaskItem != null)
				{
					inventPosition.EdoTaskItem = matchEdoTaskItem;
					unprocessedCodes.Remove(matchEdoTaskItem);
					return inventPosition;
				}
			}

			// затем Source без проблем — принимаем как Result (индивидуальный код с криптохвостом)
			var sourceCodes = unprocessedCodes
				.Where(x => x.ProductCode.Problem == ProductCodeProblem.None)
				.Where(x => x.ProductCode.SourceCode != null)
				.Where(x => x.ProductCode.SourceCode.IsInvalid == false)
				.Where(x => x.ProductCode.ResultCode == null)
				.Where(x => x.ProductCode.SourceCodeStatus != SourceProductCodeStatus.SavedToPool)
				.Where(x => x.ProductCode.SourceCode.CheckCode != null);

			foreach(var gtin in orderItem.Nomenclature.Gtins)
			{
				var matchEdoTaskItem = sourceCodes
					.Where(x => x.ProductCode.SourceCode.Gtin == gtin.GtinNumber)
					.FirstOrDefault();

				if(matchEdoTaskItem != null)
				{
					matchEdoTaskItem.ProductCode.ResultCode = matchEdoTaskItem.ProductCode.SourceCode;
					matchEdoTaskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Accepted;

					inventPosition.EdoTaskItem = matchEdoTaskItem;
					unprocessedCodes.Remove(matchEdoTaskItem);
					return inventPosition;
				}
			}

			throw new ResaleMissingCodesException($"Не найден код для номенклатуры Id {orderItem.Nomenclature.Id}");
		}

		private EdoFiscalDocument PrepareFiscalDocument(ReceiptEdoTask receiptEdoTask, int documentIndex)
		{
			var order = receiptEdoTask.FormalEdoRequest.Order;
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
					Contact = _edoOrderContactProvider.GetContact(order).StringValue,
					ClientInn = order.Client.INN,
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

			var vatRateVersion = nomenclature.GetEffectiveVatRateVersion(organization, order.DeliveryDate);

			if(vatRateVersion == null)
			{
				throw new InvalidOperationException($"У товара #{nomenclature.Id} отсутствует версия НДС на дату доставки #{order.DeliveryDate}");
			}

			inventPosition.Vat = vatRateVersion.VatRate.ToFiscalVat();

			return inventPosition;
		}

		private async Task<bool> PrepareIndustryRequisite(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			var seller = receiptEdoTask.FormalEdoRequest.Order.Contract.Organization;
			var cashBoxToken = seller.CashBoxTokenFromTrueMark;
			var regulatoryDocument = _uow.GetById<FiscalIndustryRequisiteRegulatoryDocument>(
				_edoReceiptSettings.IndustryRequisiteRegulatoryDocumentId);

			bool isValid = true;
			var invalidTaskItems = new List<EdoTaskItem>();

			foreach(var fiscalDocument in receiptEdoTask.FiscalDocuments)
			{
				var codesToCheck1260 = fiscalDocument.InventPositions
					.Where(x => x.EdoTaskItem?.ProductCode?.ResultCode != null || x.GroupCode != null)
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

				if(cashBoxToken == null)
				{
					await _edoProblemRegistrar.RegisterCustomProblem<IndustryRequisiteMissingOrganizationToken>(
						receiptEdoTask,
						cancellationToken,
						$"Отсутствует токен для организации Id {seller.Id}");
					return false;
				}

				if(regulatoryDocument == null)
				{
					await _edoProblemRegistrar.RegisterCustomProblem<IndustryRequisiteRegualtoryDocumentIsMissing>(
						receiptEdoTask,
						cancellationToken);
					return false;
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
					return false;
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
					var taskItems = invalidCodes
						.Select(x => codesToCheck1260[x.Cis].EdoTaskItem)
						.Where(x => x != null);

					invalidTaskItems.AddRange(taskItems);
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
				_edoProblemRegistrar.SolveCustomProblem<IndustryRequisiteHasInvalidCodes>(receiptEdoTask);
			}
			else
			{
				await _edoProblemRegistrar.RegisterCustomProblem<IndustryRequisiteHasInvalidCodes>(
						receiptEdoTask,
						invalidTaskItems,
						cancellationToken);
			}

			return isValid;
		}

		private void TryRecalculateOrderVat(ReceiptEdoTask receiptEdoTask)
		{
			try
			{
				var order = receiptEdoTask.FormalEdoRequest.Order;
				var firstInventPosition = receiptEdoTask
					.FiscalDocuments
					.FirstOrDefault()?
					.InventPositions
					.FirstOrDefault();

				if(firstInventPosition != null)
				{
					var vatValue = firstInventPosition.Vat.ToAddedVat();

					if(order.OrderItems.Any(x => x.ValueAddedTax != vatValue))
					{
						order.RecalculateVat();
					}
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при пересчете НДС в заказе");
			}
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
