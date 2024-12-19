using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Data.Interfaces.Employees;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Domain.Documents;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Errors;
using Vodovoz.Settings.Warehouse;
using VodovozBusiness.Services.TrueMark;
using CarLoadDocumentErrors = Vodovoz.Errors.Stores.CarLoadDocument;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCode;

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

		public CarLoadDocumentProcessingErrorsChecker(
			ILogger<CarLoadDocumentProcessingErrorsChecker> logger,
			IUnitOfWork uow,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			ICarLoadDocumentRepository carLoadDocumentRepository,
			IEmployeeWithLoginRepository employeeWithLoginRepository,
			ICarLoadDocumentLoadingProcessSettings carLoadDocumentLoadingProcessSettings,
			IGenericRepository<OrderEntity> orderRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_carLoadDocumentRepository = carLoadDocumentRepository ?? throw new ArgumentNullException(nameof(carLoadDocumentRepository));
			_employeeWithLoginRepository = employeeWithLoginRepository ?? throw new ArgumentNullException(nameof(employeeWithLoginRepository));
			_carLoadDocumentLoadingProcessSettings = carLoadDocumentLoadingProcessSettings;
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));

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
			var isNotAllCodesAdded = carLoadDocument.Items
				.Where(x =>
					x.OrderId != null
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

		public async Task<Result> IsTrueMarkCodeCanBeAdded(
			int orderId,
			int nomenclatureId,
			TrueMarkWaterIdentificationCode trueMarkWaterCode,
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

			result = IsScannedCodeValid(trueMarkWaterCode);

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

			result = IsTrueMarkCodeNotUsedAndHasRequiredGtin(trueMarkWaterCode, documentItemToEdit.Nomenclature.Gtin);

			if(result.IsFailure)
			{
				return result;
			}

			return await IsTrueMarkCodeIntroducedAndHasCorrectInn(trueMarkWaterCode, cancellationToken);
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

			return await IsTrueMarkCodeIntroducedAndHasCorrectInn(newTrueMarkWaterCode, cancellationToken);
		}

		public Result IsOrderNeedIndividualSetOnLoad(int orderId)
		{
			var order = _orderRepository.Get(_uow, o => o.Id == orderId).FirstOrDefault();

			if(order is null)
			{
				var error = CarLoadDocumentErrors.CreateOrderNotFound(orderId);
				LogError(error);
				return Result.Failure(error);
			}

			if(!order.IsNeedIndividualSetOnLoad)
			{
				var error = CarLoadDocumentErrors.CreateOrderNoNeedIndividualSetOnLoad(orderId);
				LogError(error);
				return Result.Failure(error);
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
				.Any(x => x.GTIN == trueMarkWaterCode.GTIN && x.SerialNumber == trueMarkWaterCode.SerialNumber && x.CheckCode == trueMarkWaterCode.CheckCode))
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
			string nomenclatureGtin)
		{
			var result = IsTrueMarkCodeNotUsed(trueMarkWaterCode);

			if(result.IsFailure)
			{
				return result;
			}

			return IsTrueMarkCodeGtinsEqualsNomenclatureGtin(trueMarkWaterCode, nomenclatureGtin);
		}

		private Result IsTrueMarkCodeGtinsEqualsNomenclatureGtin(TrueMarkWaterIdentificationCode trueMarkWaterCode, string nomenclatureGtin)
		{
			if(trueMarkWaterCode.GTIN != nomenclatureGtin)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin(trueMarkWaterCode.RawCode);
				LogError(error);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsTrueMarkCodesHasEqualGtins(TrueMarkWaterIdentificationCode trueMarkWaterCode1, TrueMarkWaterIdentificationCode trueMarkWaterCode2)
		{
			if(trueMarkWaterCode1.GTIN != trueMarkWaterCode2.GTIN)
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

		private async Task<Result> IsTrueMarkCodeIntroducedAndHasCorrectInn(
			TrueMarkWaterIdentificationCode trueMarkWaterCode,
			CancellationToken cancellationToken)
		{
			return await _trueMarkWaterCodeService.IsTrueMarkCodeIntroducedAndHasCorrectInn(trueMarkWaterCode, cancellationToken);
		}

		private void LogError(Error error)
		{
			_logger.LogError(error.Message);
		}
	}
}
