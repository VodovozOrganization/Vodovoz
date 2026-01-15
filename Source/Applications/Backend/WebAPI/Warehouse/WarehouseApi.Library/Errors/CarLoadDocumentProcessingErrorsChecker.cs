using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentNHibernate.Utils;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Data.Interfaces.Employees;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Settings.Warehouse;
using VodovozBusiness.Services.TrueMark;
using CarLoadDocumentErrors = Vodovoz.Errors.Stores.CarLoadDocumentErrors;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCodeErrors;

namespace WarehouseApi.Library.Errors
{
	public class CarLoadDocumentProcessingErrorsChecker
	{
		private readonly ILogger<CarLoadDocumentProcessingErrorsChecker> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly ICarLoadDocumentRepository _carLoadDocumentRepository;
		private readonly IEmployeeWithLoginRepository _employeeWithLoginRepository;
		private readonly ICarLoadDocumentLoadingProcessSettings _carLoadDocumentLoadingProcessSettings;
		private readonly IGenericRepository<OrderEntity> _orderRepository;
		private readonly ICounterpartyEdoAccountEntityController _counterpartyEdoAccountController;

		public CarLoadDocumentProcessingErrorsChecker(
			ILogger<CarLoadDocumentProcessingErrorsChecker> logger,
			IUnitOfWork uow,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			ICarLoadDocumentRepository carLoadDocumentRepository,
			IEmployeeWithLoginRepository employeeWithLoginRepository,
			ICarLoadDocumentLoadingProcessSettings carLoadDocumentLoadingProcessSettings,
			IGenericRepository<OrderEntity> orderRepository,
			ICounterpartyEdoAccountEntityController counterpartyEdoAccountController)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_carLoadDocumentRepository = carLoadDocumentRepository ?? throw new ArgumentNullException(nameof(carLoadDocumentRepository));
			_employeeWithLoginRepository = employeeWithLoginRepository ?? throw new ArgumentNullException(nameof(employeeWithLoginRepository));
			_carLoadDocumentLoadingProcessSettings = carLoadDocumentLoadingProcessSettings;
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_counterpartyEdoAccountController =
				counterpartyEdoAccountController ?? throw new ArgumentNullException(nameof(counterpartyEdoAccountController));
		}

		public Result IsCarLoadDocumentLoadingCanBeStarted(
			CarLoadDocumentEntity carLoadDocument,
			int documentId)
		{
			var result = IsCarLoadDocumentNotNull(carLoadDocument, documentId);

			if(result.IsFailure)
			{
				return result;
			}

			return IsCarLoadDocumentLoadOperationStateNotStartedOrInProgress(carLoadDocument, documentId);
		}

		public Result IsCarLoadDocumentLoadingCanBeDone(
			CarLoadDocumentEntity carLoadDocument,
			int documentId)
		{
			var result = IsCarLoadDocumentNotNull(carLoadDocument, documentId);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsCarLoadDocumentLoadOperationStateInProgress(carLoadDocument, documentId);

			if(result.IsFailure)
			{
				return result;
			}

			return IsAllTrueMarkCodesInCarLoadDocumentAdded(carLoadDocument, documentId);
		}

		private Result IsCarLoadDocumentNotNull(CarLoadDocumentEntity carLoadDocument, int documentId)
		{
			if(carLoadDocument is null)
			{
				var error = CarLoadDocumentErrors.CreateDocumentNotFound(documentId);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsCarLoadDocumentLoadOperationStateNotStarted(CarLoadDocument carLoadDocument, int documentId)
		{
			if(carLoadDocument.LoadOperationState != CarLoadDocumentLoadOperationState.NotStarted)
			{
				var error = CarLoadDocumentErrors.CreateLoadingProcessStateMustBeNotStarted(documentId);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		public Result IsEmployeeCanPickUpCarLoadDocument(int documentId, EmployeeWithLogin employee)
		{
			var lastDocumentLoadingProcessAction =
				_carLoadDocumentRepository.GetLastLoadingProcessActionByDocumentId(_uow, documentId);

			var noLoadingActionsTimeout = _carLoadDocumentLoadingProcessSettings.NoLoadingActionsTimeout;

			var isEmployeeCanPickUpCarLoadDocument =
				lastDocumentLoadingProcessAction is null
				|| lastDocumentLoadingProcessAction.PickerEmployeeId == employee?.Id
				|| DateTime.Now > lastDocumentLoadingProcessAction.ActionTime.Add(noLoadingActionsTimeout);

			if(!isEmployeeCanPickUpCarLoadDocument)
			{
				var leftToEndNoLoadingActionsTimeout = lastDocumentLoadingProcessAction.ActionTime.Add(noLoadingActionsTimeout) - DateTime.Now;
				var pickerEmployee =
					_employeeWithLoginRepository
					.GetEmployeeWithLoginById(_uow, lastDocumentLoadingProcessAction.PickerEmployeeId);

				var error = CarLoadDocumentErrors.CreateCarLoadDocumentAlreadyHasPickerError(
					documentId,
					pickerEmployee?.ShortName ?? "Сотрудник не найден",
					leftToEndNoLoadingActionsTimeout);

				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsAllTrueMarkCodesInCarLoadDocumentAdded(CarLoadDocumentEntity carLoadDocument, int documentId)
		{
			var cancelledOrdersIds = GetCarLoadDocumentCancelledOrders(carLoadDocument);

			var isNotAllCodesAdded = carLoadDocument.Items
				.Where(x =>
					x.OrderId != null
					&& !cancelledOrdersIds.Contains(x.OrderId.Value)
					&& x.Nomenclature.IsAccountableInTrueMark
					&& x.Nomenclature.Gtin != null)
				.Any(x => x.TrueMarkCodes.Count < x.Amount);

			if(isNotAllCodesAdded)
			{
				var error = CarLoadDocumentErrors.CreateNotAllTrueMarkCodesWasAddedIntoCarLoadDocument(documentId);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private IEnumerable<int> GetCarLoadDocumentCancelledOrders(CarLoadDocumentEntity carLoadDocument)
		{
			var ordersInDocument = carLoadDocument.Items
				.Where(x => x.OrderId != null)
				.Select(x => x.OrderId.Value)
				.Distinct()
				.ToList();

			var undeliveredStatuses = new OrderStatus[]
			{
				OrderStatus.NotDelivered,
				OrderStatus.DeliveryCanceled,
				OrderStatus.Canceled
			};
			
			var cancelledOrders =
				_orderRepository.Get(_uow, o => ordersInDocument.Contains(o.Id) && undeliveredStatuses.Contains(o.OrderStatus));

			return cancelledOrders.Select(o => o.Id);
		}

		private Result IsCarLoadDocumentLoadOperationStateNotStartedOrInProgress(CarLoadDocumentEntity carLoadDocument, int documentId)
		{
			if(!(carLoadDocument.LoadOperationState == CarLoadDocumentLoadOperationState.NotStarted
				|| carLoadDocument.LoadOperationState == CarLoadDocumentLoadOperationState.InProgress))
			{
				var error = CarLoadDocumentErrors.CreateLoadingProcessStateMustBeNotStartedOrInProgress(documentId);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsCarLoadDocumentLoadOperationStateInProgress(CarLoadDocumentEntity carLoadDocument, int documentId)
		{
			if(carLoadDocument.LoadOperationState != CarLoadDocumentLoadOperationState.InProgress)
			{
				var error = CarLoadDocumentErrors.CreateLoadingProcessStateMustBeInProgress(documentId);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		public Result IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(int orderId, IEnumerable<CarLoadDocumentItemEntity> documentOrderItems)
		{
			if(documentOrderItems is null || documentOrderItems.Count() == 0)
			{
				var error = CarLoadDocumentErrors.CreateCarLoadDocumentItemNotFound(orderId);
				LogError(error);
				return Result.Failure(error);
			}

			if(documentOrderItems.Select(oi => oi.Document.Id).Distinct().Count() > 1)
			{
				var error = CarLoadDocumentErrors.CreateOrderItemsExistInMultipleDocuments(orderId);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		public async Task<Result> IsTrueMarkCodesCanBeAdded(
			int orderId,
			int nomenclatureId,
			IEnumerable<TrueMarkWaterIdentificationCode> trueMarkWaterCodes,
			IEnumerable<CarLoadDocumentItemEntity> allWaterOrderItems,
			IEnumerable<CarLoadDocumentItemEntity> itemsHavingRequiredNomenclature,
			CarLoadDocumentItemEntity documentItemToEdit,
			CancellationToken cancellationToken
			)
		{
			var result = IsOrderNeedIndividualSetOnLoad(orderId);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsDocumentItemToEditNotNull(documentItemToEdit, orderId);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsCarLoadDocumentLoadOperationStateInProgress(documentItemToEdit.Document, documentItemToEdit.Document.Id);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(orderId, allWaterOrderItems);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsSingleItemHavingRequiredOrderAndNomenclatureExists(orderId, nomenclatureId, itemsHavingRequiredNomenclature);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsNotAllProductsHasTrueMarkCode(orderId, nomenclatureId, documentItemToEdit);

			if(result.IsFailure)
			{
				return result;
			}

			foreach(var trueMarkWaterCode in trueMarkWaterCodes)
			{
				result = IsScannedCodeValid(trueMarkWaterCode);

				if(result.IsFailure)
				{
					return result;
				}

				result = IsTrueMarkCodeNotUsedAndHasRequiredGtin(trueMarkWaterCode, documentItemToEdit.Nomenclature.Gtins.Select(x => x.GtinNumber));

				if(result.IsFailure)
				{
					return result;
				}
			}

			return await _trueMarkWaterCodeService.IsAllTrueMarkCodesValid(trueMarkWaterCodes, cancellationToken);
		}

		public async Task<Result> IsTrueMarkCodesCanBeDeleted(
			int orderId,
			CarLoadDocumentItemEntity documentItemToEdit
		)
		{
			var result = IsDocumentItemToEditNotNull(documentItemToEdit, orderId);

			if(result.IsFailure)
			{
				return result;
			}
			
			result = IsCarLoadDocumentLoadOperationStateInProgress(documentItemToEdit.Document, documentItemToEdit.Document.Id);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsOrderStatusBeforeOnTheWay(orderId);
			
			if (result.IsFailure)
			{
				return result;
			}
			
			return Result.Success();
		}

		public async Task<Result> IsTrueMarkCodeCanBeChanged(
			int orderId,
			int nomenclatureId,
			TrueMarkWaterIdentificationCode oldTrueMarkWaterCode,
			TrueMarkWaterIdentificationCode newTrueMarkWaterCode,
			IEnumerable<CarLoadDocumentItemEntity> allWaterOrderItems,
			IEnumerable<CarLoadDocumentItemEntity> itemsHavingRequiredNomenclature,
			CarLoadDocumentItemEntity documentItemToEdit,
			CancellationToken cancellationToken)
		{
			var result = IsOrderNeedIndividualSetOnLoad(orderId);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsDocumentItemToEditNotNull(documentItemToEdit, orderId);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsCarLoadDocumentLoadOperationStateInProgress(documentItemToEdit.Document, documentItemToEdit.Document.Id);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsScannedCodeValid(oldTrueMarkWaterCode);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsScannedCodeValid(newTrueMarkWaterCode);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsTrueMarkCodesHasEqualGtins(oldTrueMarkWaterCode, newTrueMarkWaterCode);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(orderId, allWaterOrderItems);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsSingleItemHavingRequiredOrderAndNomenclatureExists(orderId, nomenclatureId, itemsHavingRequiredNomenclature);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsProductsHavingRequiredTrueMarkCodeExists(documentItemToEdit, oldTrueMarkWaterCode);

			if(result.IsFailure)
			{
				return result;
			}

			result = IsTrueMarkCodeNotUsed(newTrueMarkWaterCode);

			if(result.IsFailure)
			{
				return result;
			}

			return await _trueMarkWaterCodeService.IsTrueMarkCodeValid(newTrueMarkWaterCode, cancellationToken);
		}

		public Result IsOrderNeedIndividualSetOnLoad(int orderId)
		{
			var order = _orderRepository.Get(_uow, o => o.Id == orderId).FirstOrDefault();

			if(order is null)
			{
				_logger.LogWarning("Заказ {OrderId} не найден", orderId);
				return CarLoadDocumentErrors.CreateOrderNotFound(orderId);
			}

			if(!order.IsNeedIndividualSetOnLoad(_counterpartyEdoAccountController) && !order.IsNeedIndividualSetOnLoadForTender)
			{
				if(order.PaymentType != PaymentType.Cashless)
				{
					_logger.LogWarning("В заказе {OrderId} тип оплаты не безналичный, сканирование не требуется", orderId);
					return CarLoadDocumentErrors.CreateOrderNoNeedIndividualSetOnLoadPaymentIsNotCashless(orderId);
				}

				if(order.Client is null)
				{
					_logger.LogWarning("В заказе {OrderId} не указан контрагент", orderId);
					return CarLoadDocumentErrors.CreateOrderNoNeedIndividualSetOnLoadClientIsNotSet(orderId);
				}

				var edoAccount = _counterpartyEdoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(
					order.Client, order.Contract.Organization.Id);

				if(edoAccount.ConsentForEdoStatus != ConsentForEdoStatus.Agree)
				{
					_logger.LogWarning("В заказе {OrderId} у клиента нет согласия на отправки документов по ЭДО, сканирование не требуется", orderId);
					return CarLoadDocumentErrors.CreateOrderNoNeedIndividualSetOnLoadConsentForEdoIsNotAgree(orderId);
				}

				if(order.Client.OrderStatusForSendingUpd != OrderStatusForSendingUpd.EnRoute)
				{
					_logger.LogWarning("Заказ {OrderId} не в статусе в пути для ЭДО", orderId);
					return CarLoadDocumentErrors.CreateOrderNoNeedIndividualSetOnLoadOrderIsNotEnRoute(orderId);
				}
			}

			return Result.Success();
		}

		public Result IsOrderStatusBeforeOnTheWay(int orderId)
		{
			var order = _orderRepository.Get(_uow, o => o.Id == orderId).FirstOrDefault();

			if(order is null)
			{
				_logger.LogWarning("Заказ {OrderId} не найден", orderId);
				return CarLoadDocumentErrors.CreateOrderNotFound(orderId);
			}

			var banStatuses = new[] { OrderStatus.OnTheWay, OrderStatus.DeliveryCanceled,  OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.NotDelivered, OrderStatus.Closed};

			if(banStatuses.Any(status => status == order.OrderStatus))
			{
				_logger.LogWarning("Заказ {OrderId} имеет статус в котором нельзя изменять коды", orderId);
				return CarLoadDocumentErrors.CreateOrderBadStatus(orderId, order.OrderStatus);
			}
			
			return Result.Success();
		}
		
		private Result IsDocumentItemToEditNotNull(CarLoadDocumentItemEntity documentItemToEdit, int orderId)
		{
			if(documentItemToEdit is null)
			{
				var error = CarLoadDocumentErrors.CreateCarLoadDocumentItemNotFound(orderId);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsSingleItemHavingRequiredOrderAndNomenclatureExists(
			int orderId,
			int nomenclatureId,
			IEnumerable<CarLoadDocumentItemEntity> documentNomenclatureOrderItems)
		{
			if(documentNomenclatureOrderItems.Count() == 0)
			{
				var error = CarLoadDocumentErrors.CreateOrderDoesNotContainNomenclature(orderId, nomenclatureId);
				LogError(error);
				return Result.Failure(error);
			}

			if(documentNomenclatureOrderItems.Count() > 1)
			{
				var error = CarLoadDocumentErrors.CreateOrderNomenclatureExistInMultipleDocumentItems(orderId, nomenclatureId);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsNotAllProductsHasTrueMarkCode(
			int orderId,
			int nomenclatureId,
			CarLoadDocumentItemEntity carLoadDocumentItem)
		{
			if(carLoadDocumentItem.TrueMarkCodes.Count() >= carLoadDocumentItem.Amount)
			{
				var error = CarLoadDocumentErrors.CreateAllOrderNomenclatureCodesAlreadyAdded(orderId, nomenclatureId);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsProductsHavingRequiredTrueMarkCodeExists(
			CarLoadDocumentItemEntity carLoadDocumentItem,
			TrueMarkWaterIdentificationCode trueMarkWaterCode)
		{
			if(!carLoadDocumentItem
				.TrueMarkCodes.Select(x => x.SourceCode)
				.Any(x => x.Gtin == trueMarkWaterCode.Gtin && x.SerialNumber == trueMarkWaterCode.SerialNumber && x.CheckCode == trueMarkWaterCode.CheckCode))
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodeForCarLoadDocumentItemNotFound(trueMarkWaterCode.RawCode);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsScannedCodeValid(TrueMarkWaterIdentificationCode trueMarkWaterCode)
		{
			if(trueMarkWaterCode.IsInvalid)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodeStringIsNotValid(trueMarkWaterCode.RawCode);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsTrueMarkCodeNotUsedAndHasRequiredGtin(
			TrueMarkWaterIdentificationCode trueMarkWaterCode,
			IEnumerable<string> nomenclatureGtins)
		{
			var result = IsTrueMarkCodeNotUsed(trueMarkWaterCode);

			if(result.IsFailure)
			{
				return result;
			}

			return IsTrueMarkCodeGtinsEqualsNomenclatureGtin(trueMarkWaterCode, nomenclatureGtins);
		}

		private Result IsTrueMarkCodeGtinsEqualsNomenclatureGtin(TrueMarkWaterIdentificationCode trueMarkWaterCode, IEnumerable<string> nomenclatureGtins)
		{
			if(!nomenclatureGtins.Contains(trueMarkWaterCode.Gtin))
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin(trueMarkWaterCode.RawCode);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsTrueMarkCodesHasEqualGtins(TrueMarkWaterIdentificationCode trueMarkWaterCode1, TrueMarkWaterIdentificationCode trueMarkWaterCode2)
		{
			if(trueMarkWaterCode1.Gtin != trueMarkWaterCode2.Gtin)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodesGtinsNotEqual(trueMarkWaterCode1.RawCode, trueMarkWaterCode2.RawCode);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsTrueMarkCodeNotUsed(TrueMarkWaterIdentificationCode trueMarkWaterCode)
		{
			return _trueMarkWaterCodeService.IsTrueMarkWaterIdentificationCodeNotUsed(trueMarkWaterCode);
		}

		private void LogError(Error error)
		{
			_logger.LogError(error.Message);
		}
	}
}
