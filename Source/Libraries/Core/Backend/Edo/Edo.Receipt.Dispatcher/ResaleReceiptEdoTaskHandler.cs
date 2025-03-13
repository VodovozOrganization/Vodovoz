using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate.Exceptions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Library;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Settings.Edo;

namespace Edo.Receipt.Dispatcher
{
	public class ResaleReceiptEdoTaskHandler
	{
		private readonly ILogger<ResaleReceiptEdoTaskHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly EdoTaskValidator _edoTaskValidator;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly TransferRequestCreator _transferRequestCreator;
		private readonly TrueMarkTaskCodesValidator _localCodesValidator;
		private readonly TrueMarkTaskCodesValidator _trueMarkTaskCodesValidator;
		private readonly Tag1260Checker _tag1260Checker;
		private readonly IEdoReceiptSettings _edoReceiptSettings;
		private readonly IBus _messageBus;
		private readonly int _maxCodesInReceipt;

		public ResaleReceiptEdoTaskHandler(
			ILogger<ResaleReceiptEdoTaskHandler> logger,
			IUnitOfWork uow,
			EdoTaskValidator edoTaskValidator,
			EdoProblemRegistrar edoProblemRegistrar,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			TransferRequestCreator transferRequestCreator,
			TrueMarkTaskCodesValidator localCodesValidator,
			TrueMarkTaskCodesValidator trueMarkTaskCodesValidator,
			Tag1260Checker tag1260Checker,
			IEdoReceiptSettings edoReceiptSettings,
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
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));

			_maxCodesInReceipt = _edoReceiptSettings.MaxCodesInReceiptCount;
		}

		public async Task HandleResaleReceipt(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
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

			var sendReceiptMessage = new ReceiptSendEvent { ReceiptEdoTaskId = receiptEdoTask.Id };
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
		private void PrepareFiscalDocuments(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			var order = receiptEdoTask.OrderEdoRequest.Order;

			//создать немаркированные позиции
			var mainFiscalDocument = CreateUnmarkedFiscalDocument(receiptEdoTask);

			//создать маркированные позиции
			CreateMarkedFiscalDocuments(receiptEdoTask, order, mainFiscalDocument, cancellationToken);

			//создать или обновить сумму в чеках
			foreach(var fiscalDocument in receiptEdoTask.FiscalDocuments)
			{
				CreateReceiptMoneyPositions(fiscalDocument);
			}
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
			OrderEntity order,
			EdoFiscalDocument mainFiscalDocument,
			CancellationToken cancellationToken
			)
		{
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
			var receiptSum = currentFiscalDocument.InventPositions
								.Sum(x => x.OrderItems.First().Price * x.Quantity - x.DiscountSum);

			var moneyPosition = new FiscalMoneyPosition
			{
				PaymentType = FiscalPaymentType.Cash,
				Sum = receiptSum
			};

			currentFiscalDocument.MoneyPositions.Add(moneyPosition);
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
					.Where(x => x.ProductCode.ResultCode.GTIN == gtin.GtinNumber)
					.FirstOrDefault();
				if(matchEdoTaskItem != null)
				{
					inventPosition.EdoTaskItem = matchEdoTaskItem;
					unprocessedCodes.Remove(matchEdoTaskItem);
					return inventPosition;
				}
			}

			throw new InvalidOperationException($"Не найден код для номенклатуры {orderItem.Nomenclature}");
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
				Contact = GetContact(order),
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

		private async Task<bool> PrepareIndustryRequisite(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken)
		{
			var seller = receiptEdoTask.OrderEdoRequest.Order.Contract.Organization;
			var cashBoxToken = seller.CashBoxTokenFromTrueMark;
			if(cashBoxToken == null)
			{
				await _edoProblemRegistrar.RegisterCustomProblem<IndustryRequisiteMissingOrganizationToken>(
					receiptEdoTask, 
					cancellationToken,
					$"Отсутствует токен для организации Id {seller.Id}");
				return false;
			}

			var regulatoryDocument = _uow.GetById<FiscalIndustryRequisiteRegulatoryDocument>(
				_edoReceiptSettings.IndustryRequisiteRegulatoryDocumentId);
			if(regulatoryDocument == null)
			{
				await _edoProblemRegistrar.RegisterCustomProblem<IndustryRequisiteRegualtoryDocumentIsMissing>(
					receiptEdoTask,
					cancellationToken);
				return false;
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
