using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Exception.EdoExceptions;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Library;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
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
		private readonly IBus _messageBus;
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
			IBus messageBus
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
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));

			_maxCodesInReceipt = _edoReceiptSettings.MaxCodesInReceiptCount;
		}

		public async Task HandleNewReceipt(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			var order = receiptEdoTask.OrderEdoRequest.Order;
			if(order.Client.ReasonForLeaving != ReasonForLeaving.Resale)
			{
				throw new InvalidOperationException($"Попытка обработать чек с причиной выбытия " +
					$"{order.Client.ReasonForLeaving} обработчиком для {ReasonForLeaving.ForOwnNeeds}.");
			}

			var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(receiptEdoTask);

			var isValid = await _edoTaskValidator.Validate(receiptEdoTask, cancellationToken, trueMarkCodesChecker);
			if(!isValid)
			{
				return;
			}
			
			var hasManualSend = receiptEdoTask.OrderEdoRequest.Source == CustomerEdoRequestSource.Manual;

			if(!hasManualSend)
			{
				await SaveCodesToPool(receiptEdoTask, cancellationToken);
				receiptEdoTask.Status = EdoTaskStatus.Completed;
				receiptEdoTask.ReceiptStatus = EdoReceiptStatus.SavedToPool;
				receiptEdoTask.EndTime = DateTime.Now;
				await _uow.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
				await _uow.CommitAsync(cancellationToken);
				return;
			}

			PrepareFiscalDocuments(receiptEdoTask, cancellationToken);

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
			receiptEdoTask.CashboxId = receiptEdoTask.OrderEdoRequest.Order.Contract.Organization.CashBoxId;
			await _uow.SaveAsync(receiptEdoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			var sendReceiptMessage = new ReceiptReadyToSendEvent { ReceiptEdoTaskId = receiptEdoTask.Id };
			await _messageBus.Publish(sendReceiptMessage);
		}

		public async Task HandleTransferComplete(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
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
		private void PrepareFiscalDocuments(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			//создать немаркированные позиции
			var mainFiscalDocument = CreateUnmarkedFiscalDocument(receiptEdoTask);

			//создать маркированные позиции
			CreateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, cancellationToken);

			//создать или обновить сумму в чеках
			foreach(var fiscalDocument in receiptEdoTask.FiscalDocuments)
			{
				CreateReceiptMoneyPositions(fiscalDocument);
			}
		}
		
		private async Task SaveCodesToPool(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			await _saveCodesService.SaveCodesToPool(receiptEdoTask, cancellationToken);
		}

		public EdoFiscalDocument CreateUnmarkedFiscalDocument(ReceiptEdoTask receiptEdoTask)
		{
			var order = receiptEdoTask.OrderEdoRequest.Order;
			var unmarkedOrderItems = order.OrderItems
				.Where(x => x.Price != 0m)
				.Where(x => x.Count > 0m)
				.Where(x => x.Nomenclature.IsAccountableInTrueMark == false);

			var fiscalDocument = CreateFiscalDocument(receiptEdoTask);
			fiscalDocument.Index = 0;
			receiptEdoTask.FiscalDocuments.Add(fiscalDocument);

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

		private void CreateMarkedFiscalDocuments(
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

			var expandedMarkedItems = ExpandMarkedOrderItems(markedOrderItems);
			var unprocessedCodes = receiptEdoTask.Items.ToList();

			var documentIndex = mainFiscalDocument.Index;
			var currentFiscalDocument = mainFiscalDocument;
			var currentProcessingPositions = expandedMarkedItems.Skip(0).Take(_maxCodesInReceipt);
			do
			{
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
				}

				receiptEdoTask.FiscalDocuments.Add(currentFiscalDocument);

				// подготавливаем данные для следующей итерации
				documentIndex++;
				currentProcessingPositions = expandedMarkedItems
					.Skip(_maxCodesInReceipt * documentIndex)
					.Take(_maxCodesInReceipt);
				currentFiscalDocument = CreateFiscalDocument(receiptEdoTask);
				currentFiscalDocument.DocumentNumber += $"_{documentIndex}";

			} while(currentProcessingPositions.Any());
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

		private void CreateReceiptMoneyPositions(EdoFiscalDocument currentFiscalDocument)
		{
			var order = currentFiscalDocument.ReceiptEdoTask.OrderEdoRequest.Order;

			var receiptSum = currentFiscalDocument.InventPositions
				.Sum(x => x.OrderItems.First().Price * x.Quantity - x.DiscountSum);

			var moneyPosition = new FiscalMoneyPosition
			{
				PaymentType = GetPaymentType(order.PaymentType),
				Sum = receiptSum
			};

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

		/// <summary>
		/// Создает и подготавливает инвентарную позицию для одного экземпляра товара <br/>
		/// Сопоставляет по Gtin товара отсканированные коды и выбирает подходящий <br/>
		/// Создает новый код если не нашел подходящий <br/>
		/// Не подходящие коды заменяются кодами из пула
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
				.Where(x => x.ProductCode.ResultCode != null);
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

			throw new ResaleMissingCodesException($"Не найден код для номенклатуры Id {orderItem.Nomenclature.Id}");
		}

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
				Contact = _edoOrderContactProvider.GetContact(order).StringValue,
				ClientInn = order.Client.INN,
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
				OrderItems = new ObservableList<OrderItemEntity> { orderItem }
			};

			var organization = orderItem.Order.Contract?.Organization;

			var vatRateVersion = orderItem.Nomenclature.VatRateVersions.FirstOrDefault(x =>
				x.StartDate <= orderItem.Order.BillDate && (x.EndDate == null || x.EndDate > orderItem.Order.BillDate));
			
			if(vatRateVersion == null)
			{
				throw new InvalidOperationException($"У товара #{orderItem.Nomenclature.Id} отсутствует версия НДС на дату счета заказа #{orderItem.Order.Id}");
			}
			
			if(organization is null || organization.WithoutVAT || vatRateVersion.VatRate.VatRateValue == 0)
			{
				inventPosition.Vat = FiscalVat.VatFree;
			}
			else
			{
				inventPosition.Vat = vatRateVersion.VatRate.ToFiscalVat();
			}

			return inventPosition;
		}

		private async Task<bool> PrepareIndustryRequisite(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			var seller = receiptEdoTask.OrderEdoRequest.Order.Contract.Organization;
			var cashBoxToken = seller.CashBoxTokenFromTrueMark;
			var regulatoryDocument = _uow.GetById<FiscalIndustryRequisiteRegulatoryDocument>(
				_edoReceiptSettings.IndustryRequisiteRegulatoryDocumentId);

			bool isValid = true;
			var invalidTaskItems = new List<EdoTaskItem>();

			foreach(var fiscalDocument in receiptEdoTask.FiscalDocuments)
			{
				var codesToCheck1260 = fiscalDocument.InventPositions
					.Where(x => x.EdoTaskItem?.ProductCode?.ResultCode != null)
					.ToDictionary(x => x.EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260);

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
					var taskItems = invalidCodes.Select(x => codesToCheck1260[x.Cis].EdoTaskItem);
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

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
