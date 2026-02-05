using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Data.Interfaces.Employees;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Settings.Warehouse;
using CarLoadDocumentErrors = Vodovoz.Errors.Stores.CarLoadDocumentErrors;

namespace WarehouseApi.Library.Errors
{
	public class CarLoadDocumentProcessingErrorsChecker
	{
		private readonly ILogger<CarLoadDocumentProcessingErrorsChecker> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ICarLoadDocumentRepository _carLoadDocumentRepository;
		private readonly IEmployeeWithLoginRepository _employeeWithLoginRepository;
		private readonly ICarLoadDocumentLoadingProcessSettings _carLoadDocumentLoadingProcessSettings;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly ICounterpartyEdoAccountEntityController _counterpartyEdoAccountController;

		public CarLoadDocumentProcessingErrorsChecker(
			ILogger<CarLoadDocumentProcessingErrorsChecker> logger,
			IUnitOfWork uow,
			ICarLoadDocumentRepository carLoadDocumentRepository,
			IEmployeeWithLoginRepository employeeWithLoginRepository,
			ICarLoadDocumentLoadingProcessSettings carLoadDocumentLoadingProcessSettings,
			IGenericRepository<Order> orderRepository,
			ICounterpartyEdoAccountEntityController counterpartyEdoAccountController)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
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

			return IsCarLoadDocumentLoadOperationStateInProgress(carLoadDocument, documentId);
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

		public Result IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(int orderId, IEnumerable<CarLoadDocumentItem> documentOrderItems)
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

		public Result IsTrueMarkCodesCanBeAdded(
			int orderId,
			int nomenclatureId,
			IEnumerable<CarLoadDocumentItem> allWaterOrderItems,
			IEnumerable<CarLoadDocumentItem> itemsHavingRequiredNomenclature,
			CarLoadDocumentItem documentItemToEdit
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

			return result;
		}

		public Result IsCanChangeTrueMarkCode(
			int orderId,
			int nomenclatureId,
			IEnumerable<CarLoadDocumentItem> allWaterOrderItems,
			IEnumerable<CarLoadDocumentItem> itemsHavingRequiredNomenclature,
			CarLoadDocumentItem documentItemToEdit)
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

			return result;
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

				var edoAccount = order.Client.DefaultEdoAccount(order.Contract.Organization.Id);

				if(edoAccount == null || edoAccount.ConsentForEdoStatus != ConsentForEdoStatus.Agree)
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

		private void LogError(Error error)
		{
			_logger.LogError(error.Message);
		}
	}
}
