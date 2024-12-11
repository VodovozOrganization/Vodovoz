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
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Errors;
using Vodovoz.Models.TrueMark;
using Vodovoz.Settings.Warehouse;
using CarLoadDocumentErrors = Vodovoz.Errors.Stores.CarLoadDocument;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCode;

namespace WarehouseApi.Library.Errors
{
	public class CarLoadDocumentProcessingErrorsChecker
	{
		private readonly ILogger<CarLoadDocumentProcessingErrorsChecker> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly ICarLoadDocumentRepository _carLoadDocumentRepository;
		private readonly IEmployeeWithLoginRepository _employeeWithLoginRepository;
		private readonly ICarLoadDocumentLoadingProcessSettings _carLoadDocumentLoadingProcessSettings;
		private readonly IGenericRepository<OrderEntity> _orderRepository;
		private readonly TrueMarkCodesChecker _trueMarkCodesChecker;

		public CarLoadDocumentProcessingErrorsChecker(
			ILogger<CarLoadDocumentProcessingErrorsChecker> logger,
			IUnitOfWork uow,
			ITrueMarkRepository trueMarkRepository,
			ICarLoadDocumentRepository carLoadDocumentRepository,
			IEmployeeWithLoginRepository employeeWithLoginRepository,
			ICarLoadDocumentLoadingProcessSettings carLoadDocumentLoadingProcessSettings,
			IGenericRepository<OrderEntity> orderRepository,
			TrueMarkCodesChecker trueMarkCodesChecker)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_carLoadDocumentRepository = carLoadDocumentRepository ?? throw new ArgumentNullException(nameof(carLoadDocumentRepository));
			_employeeWithLoginRepository = employeeWithLoginRepository ?? throw new ArgumentNullException(nameof(employeeWithLoginRepository));
			_carLoadDocumentLoadingProcessSettings = carLoadDocumentLoadingProcessSettings;
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_trueMarkCodesChecker = trueMarkCodesChecker ?? throw new ArgumentNullException(nameof(trueMarkCodesChecker));
		}

		public bool IsCarLoadDocumentLoadingCanBeStarted(
			CarLoadDocumentEntity carLoadDocument,
			int documentId,
			out Error error)
		{
			return IsCarLoadDocumentNotNull(carLoadDocument, documentId, out error)
				&& IsCarLoadDocumentLoadOperationStateNotStartedOrInProgress(carLoadDocument, documentId, out error);
		}

		public bool IsCarLoadDocumentLoadingCanBeDone(
			CarLoadDocumentEntity carLoadDocument,
			int documentId,
			out Error error)
		{
			return IsCarLoadDocumentNotNull(carLoadDocument, documentId, out error)
				&& IsCarLoadDocumentLoadOperationStateInProgress(carLoadDocument, documentId, out error)
				&& IsAllTrueMarkCodesInCarLoadDocumentAdded(carLoadDocument, documentId, out error);
		}

		private bool IsCarLoadDocumentNotNull(CarLoadDocumentEntity carLoadDocument, int documentId, out Error error)
		{
			error = null;

			if(carLoadDocument is null)
			{
				error = CarLoadDocumentErrors.CreateDocumentNotFound(documentId);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsCarLoadDocumentLoadOperationStateNotStarted(CarLoadDocument carLoadDocument, int documentId, out Error error)
		{
			error = null;

			if(carLoadDocument.LoadOperationState != CarLoadDocumentLoadOperationState.NotStarted)
			{
				error = CarLoadDocumentErrors.CreateLoadingProcessStateMustBeNotStarted(documentId);
				LogError(error);
				return false;
			}

			return true;
		}

		public bool IsEmployeeCanPickUpCarLoadDocument(int documentId, EmployeeWithLogin employee, out Error error)
		{
			error = null;

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

				error = CarLoadDocumentErrors.CreateCarLoadDocumentAlreadyHasPickerError(
					documentId,
					pickerEmployee?.ShortName ?? "Сотрудник не найден",
					leftToEndNoLoadingActionsTimeout);

				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsAllTrueMarkCodesInCarLoadDocumentAdded(CarLoadDocumentEntity carLoadDocument, int documentId, out Error error)
		{
			error = null;

			var isNotAllCodesAdded = carLoadDocument.Items
				.Where(x =>
					x.OrderId != null
					&& x.Nomenclature.IsAccountableInTrueMark
					&& x.Nomenclature.Gtin != null)
				.Any(x => x.TrueMarkCodes.Count < x.Amount);

			if(isNotAllCodesAdded)
			{
				error = CarLoadDocumentErrors.CreateNotAllTrueMarkCodesWasAddedIntoCarLoadDocument(documentId);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsCarLoadDocumentLoadOperationStateNotStartedOrInProgress(CarLoadDocumentEntity carLoadDocument, int documentId, out Error error)
		{
			error = null;

			if(!(carLoadDocument.LoadOperationState == CarLoadDocumentLoadOperationState.NotStarted
				|| carLoadDocument.LoadOperationState == CarLoadDocumentLoadOperationState.InProgress))
			{
				error = CarLoadDocumentErrors.CreateLoadingProcessStateMustBeNotStartedOrInProgress(documentId);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsCarLoadDocumentLoadOperationStateInProgress(CarLoadDocumentEntity carLoadDocument, int documentId, out Error error)
		{
			error = null;

			if(carLoadDocument.LoadOperationState != CarLoadDocumentLoadOperationState.InProgress)
			{
				error = CarLoadDocumentErrors.CreateLoadingProcessStateMustBeInProgress(documentId);
				LogError(error);
				return false;
			}

			return true;
		}

		public bool IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(int orderId, IEnumerable<CarLoadDocumentItemEntity> documentOrderItems, out Error error)
		{
			error = null;

			if(documentOrderItems is null || documentOrderItems.Count() == 0)
			{
				error = CarLoadDocumentErrors.CreateCarLoadDocumentItemNotFound(orderId);
				LogError(error);
				return false;
			}

			if(documentOrderItems.Select(oi => oi.Document.Id).Distinct().Count() > 1)
			{
				error = CarLoadDocumentErrors.CreateOrderItemsExistInMultipleDocuments(orderId);
				LogError(error);
				return false;
			}

			return true;
		}

		public bool IsTrueMarkCodeCanBeAdded(
			int orderId,
			int nomenclatureId,
			string scannedCode,
			bool isScannedCodeValid,
			TrueMarkWaterCode trueMarkCode,
			IEnumerable<CarLoadDocumentItemEntity> allWaterOrderItems,
			IEnumerable<CarLoadDocumentItemEntity> itemsHavingRequiredNomenclature,
			CarLoadDocumentItemEntity documentItemToEdit,
			CancellationToken cancellationToken,
			out Error error)
		{
			return IsOrderNeedIndividualSetOnLoad(orderId, out error)
				&& IsDocumentItemToEditNotNull(documentItemToEdit, orderId, out error)
				&& IsCarLoadDocumentLoadOperationStateInProgress(documentItemToEdit.Document, documentItemToEdit.Document.Id, out error)
				&& IsScannedCodeValid(scannedCode, isScannedCodeValid, out error)
				&& IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(orderId, allWaterOrderItems, out error)
				&& IsSingleItemHavingRequiredOrderAndNomenclatureExists(orderId, nomenclatureId, itemsHavingRequiredNomenclature, out error)
				&& IsNotAllProductsHasTrueMarkCode(orderId, nomenclatureId, documentItemToEdit, out error)
				&& IsTrueMarkCodeNotExistAndHasRequiredGtin(trueMarkCode, documentItemToEdit.Nomenclature.Gtin, scannedCode, out error)
				&& IsTrueMarkCodeIntroduced(trueMarkCode, cancellationToken, out error);
		}

		public bool IsTrueMarkCodeCanBeChanged(
			int orderId,
			int nomenclatureId,
			string oldScannedCode,
			bool isOldScannedCodeValid,
			TrueMarkWaterCode oldTrueMarkCode,
			string newScannedCode,
			bool isNewScannedCodeValid,
			TrueMarkWaterCode newTrueMarkCode,
			IEnumerable<CarLoadDocumentItemEntity> allWaterOrderItems,
			IEnumerable<CarLoadDocumentItemEntity> itemsHavingRequiredNomenclature,
			CarLoadDocumentItemEntity documentItemToEdit,
			CancellationToken cancellationToken,
			out Error error)
		{
			return IsOrderNeedIndividualSetOnLoad(orderId, out error)
				&& IsDocumentItemToEditNotNull(documentItemToEdit, orderId, out error)
				&& IsCarLoadDocumentLoadOperationStateInProgress(documentItemToEdit.Document, documentItemToEdit.Document.Id, out error)
				&& IsScannedCodeValid(oldScannedCode, isOldScannedCodeValid, out error)
				&& IsScannedCodeValid(newScannedCode, isNewScannedCodeValid, out error)
				&& IsTrueMarkCodesHasEqualGtins(oldTrueMarkCode, newTrueMarkCode, out error)
				&& IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(orderId, allWaterOrderItems, out error)
				&& IsSingleItemHavingRequiredOrderAndNomenclatureExists(orderId, nomenclatureId, itemsHavingRequiredNomenclature, out error)
				&& IsProductsHavingRequiredTrueMarkCodeExists(documentItemToEdit, oldTrueMarkCode, out error)
				&& IsTrueMarkCodeNotExists(newTrueMarkCode, newScannedCode, out error)
				&& IsTrueMarkCodeIntroduced(newTrueMarkCode, cancellationToken, out error);
		}

		public bool IsOrderNeedIndividualSetOnLoad(int orderId, out Error error)
		{
			error = null;

			var order = _orderRepository.Get(_uow, o => o.Id == orderId).FirstOrDefault();

			if(order is null)
			{
				error = CarLoadDocumentErrors.CreateOrderNotFound(orderId);
				LogError(error);
				return false;
			}

			if(!order.IsNeedIndividualSetOnLoad)
			{
				error = CarLoadDocumentErrors.CreateOrderNoNeedIndividualSetOnLoad(orderId);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsDocumentItemToEditNotNull(CarLoadDocumentItemEntity documentItemToEdit, int orderId, out Error error)
		{
			error = null;

			if(documentItemToEdit is null)
			{
				error = CarLoadDocumentErrors.CreateCarLoadDocumentItemNotFound(orderId);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsSingleItemHavingRequiredOrderAndNomenclatureExists(
			int orderId,
			int nomenclatureId,
			IEnumerable<CarLoadDocumentItemEntity> documentNomenclatureOrderItems,
			out Error error)
		{
			error = null;

			if(documentNomenclatureOrderItems.Count() == 0)
			{
				error = CarLoadDocumentErrors.CreateOrderDoesNotContainNomenclature(orderId, nomenclatureId);
				LogError(error);
				return false;
			}

			if(documentNomenclatureOrderItems.Count() > 1)
			{
				error = CarLoadDocumentErrors.CreateOrderNomenclatureExistInMultipleDocumentItems(orderId, nomenclatureId);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsNotAllProductsHasTrueMarkCode(
			int orderId,
			int nomenclatureId,
			CarLoadDocumentItemEntity carLoadDocumentItem,
			out Error error)
		{
			error = null;

			if(carLoadDocumentItem.TrueMarkCodes.Count() >= carLoadDocumentItem.Amount)
			{
				error = CarLoadDocumentErrors.CreateAllOrderNomenclatureCodesAlreadyAdded(orderId, nomenclatureId);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsProductsHavingRequiredTrueMarkCodeExists(
			CarLoadDocumentItemEntity carLoadDocumentItem,
			TrueMarkWaterCode trueMarkCode,
			out Error error)
		{
			error = null;

			if(!carLoadDocumentItem
				.TrueMarkCodes.Select(x => x.SourceCode)
				.Any(x => x.GTIN == trueMarkCode.GTIN && x.SerialNumber == trueMarkCode.SerialNumber && x.CheckCode == trueMarkCode.CheckCode))
			{
				error = TrueMarkCodeErrors.CreateTrueMarkCodeForCarLoadDocumentItemNotFound(trueMarkCode.SourceCode);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsScannedCodeValid(string scannedCode, bool isScannedCodeValid, out Error error)
		{
			error = null;

			if(!isScannedCodeValid)
			{
				error = TrueMarkCodeErrors.CreateTrueMarkCodeStringIsNotValid(scannedCode);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsTrueMarkCodeNotExistAndHasRequiredGtin(
			TrueMarkWaterCode trueMarkCode,
			string nomenclatureGtin,
			string scannedCode,
			out Error error)
		{
			return IsTrueMarkCodeNotExists(trueMarkCode, scannedCode, out error)
				&& IsTrueMarkCodeGtinsEqualsNomenclatureGtin(trueMarkCode, nomenclatureGtin, scannedCode, out error);
		}

		private bool IsTrueMarkCodeGtinsEqualsNomenclatureGtin(TrueMarkWaterCode trueMarkCode, string nomenclatureGtin, string scannedCode, out Error error)
		{
			error = null;

			if(trueMarkCode.GTIN != nomenclatureGtin)
			{
				error = TrueMarkCodeErrors.CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin(scannedCode);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsTrueMarkCodesHasEqualGtins(TrueMarkWaterCode trueMarkCode1, TrueMarkWaterCode trueMarkCode2, out Error error)
		{
			error = null;

			if(trueMarkCode1.GTIN != trueMarkCode2.GTIN)
			{
				error = TrueMarkCodeErrors.CreateTrueMarkCodesGtinsNotEqual(trueMarkCode1.SourceCode, trueMarkCode2.SourceCode);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsTrueMarkCodeNotExists(TrueMarkWaterCode trueMarkCode, string scannedCode, out Error error)
		{
			error = null;

			//TODO
			//Пока проверка отсутствия кода осуществляется только проверкой наличия кода в таблице true_mark_identification_code
			//Если код отсутствует в таблице, то его можно добавить.
			//Но эта логика не совсем верна. Код может быть добавлен в таблицу, но в чеках не задействован. То есть мы откидываем код, который можно добавить
			//Но при этом, т.к. код уже есть в таблице, то он может быть доступен в пуле кодов, т.е. в любой момент прикрепиться к чеку.
			//И тогда получится, что код и к чеку прикрепился и к документу погрузки
			//Логика проверки доступности кода будет исправлена в дальнейшем, по мере внедрения нового функционала привязки кодов к чекам товаров
			// из МЛ и документов самовывоза
			var existingDuplicatedCodes =
				_trueMarkRepository
				.GetTrueMarkCodeDuplicates(_uow, trueMarkCode.GTIN, trueMarkCode.SerialNumber, trueMarkCode.CheckCode);

			if(existingDuplicatedCodes.Count() > 0)
			{
				error = TrueMarkCodeErrors.CreateTrueMarkCodeIsAlreadyExists(scannedCode);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsTrueMarkCodeIntroduced(TrueMarkWaterCode trueMarkCode, CancellationToken cancellationToken, out Error error)
		{
			error = null;

			var waterCode = new TrueMarkWaterIdentificationCode
			{
				IsInvalid = false,
				RawCode = trueMarkCode.SourceCode.Substring(0, Math.Min(255, trueMarkCode.SourceCode.Length)),
				GTIN = trueMarkCode.GTIN,
				SerialNumber = trueMarkCode.SerialNumber,
				CheckCode = trueMarkCode.CheckCode
			};

			try
			{
				var checkResults =
					_trueMarkCodesChecker.CheckCodesAsync(new List<TrueMarkWaterIdentificationCode> { waterCode }, cancellationToken)
					.Result
					.FirstOrDefault();

				if(checkResults is null || !checkResults.Introduced)
				{
					error = TrueMarkCodeErrors.TrueMarkCodeIsNotIntroduced;
					LogError(error);
					return false;
				}

				return true;
			}
			catch(Exception ex)
			{
				error = TrueMarkCodeErrors.CreateTrueMarkApiRequestError(
					"При выполнении запроса к API ЧЗ для проверки кода возникла непредвиденная ошибка. " +
					"Обратитесь в техподдержку");
				_logger.LogError(ex, error.Message);
				return false;
			}
		}

		private void LogError(Error error)
		{
			_logger.LogError(error.Message);
		}
	}
}
