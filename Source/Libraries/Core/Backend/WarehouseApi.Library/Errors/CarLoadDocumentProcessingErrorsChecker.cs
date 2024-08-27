using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Errors;
using Vodovoz.Models.TrueMark;
using CarLoadDocumentErrors = Vodovoz.Errors.Stores.CarLoadDocument;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCode;

namespace WarehouseApi.Library.Errors
{
	public class CarLoadDocumentProcessingErrorsChecker
	{
		private readonly ILogger<CarLoadDocumentProcessingErrorsChecker> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly TrueMarkCodesChecker _trueMarkCodesChecker;

		public CarLoadDocumentProcessingErrorsChecker(
			ILogger<CarLoadDocumentProcessingErrorsChecker> logger,
			IUnitOfWork uow,
			ITrueMarkRepository trueMarkRepository,
			TrueMarkCodesChecker trueMarkCodesChecker)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_trueMarkCodesChecker = trueMarkCodesChecker ?? throw new ArgumentNullException(nameof(trueMarkCodesChecker));
		}

		public bool IsCarLoadDocumentLoadOperationStateCanBeSetInProgress(CarLoadDocument carLoadDocument, int documentId, out Error error)
		{
			return IsCarLoadDocumentNotNull(carLoadDocument, documentId, out error)
				&& IsCarLoadDocumentLoadOperationStateNotStarted(carLoadDocument, documentId, out error);
		}

		public bool IsCarLoadDocumentLoadOperationStateCanBeSetInDone(CarLoadDocument carLoadDocument, int documentId, out Error error)
		{
			return IsCarLoadDocumentNotNull(carLoadDocument, documentId, out error)
				&& IsCarLoadDocumentLoadOperationStateInProgress(carLoadDocument, documentId, out error)
				&& IsAllTrueMarkCodesInCarLoadDocumentAdded(carLoadDocument, documentId, out error);
		}

		private bool IsCarLoadDocumentNotNull(CarLoadDocument carLoadDocument, int documentId, out Error error)
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

		private bool IsAllTrueMarkCodesInCarLoadDocumentAdded(CarLoadDocument carLoadDocument, int documentId, out Error error)
		{
			error = null;

			var isNotAllCodesAdded = carLoadDocument.Items
				.Where(x => x.OrderId != null && x.Nomenclature.Category == Vodovoz.Domain.Goods.NomenclatureCategory.water)
				.Any(x => x.TrueMarkCodes.Count < x.Amount);

			if(isNotAllCodesAdded)
			{
				error = CarLoadDocumentErrors.CreateNotAllTrueMarkCodesWasAddedIntoCarLoadDocument(documentId);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsCarLoadDocumentLoadOperationStateInProgress(CarLoadDocument carLoadDocument, int documentId, out Error error)
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

		public bool IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(int orderId, IList<CarLoadDocumentItem> documentOrderItems, out Error error)
		{
			error = null;

			if(documentOrderItems is null || documentOrderItems.Count == 0)
			{
				error = CarLoadDocumentErrors.CreateOrderNotFound(orderId);
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
			IList<CarLoadDocumentItem> allWaterOrderItems,
			IEnumerable<CarLoadDocumentItem> itemsHavingRequiredNomenclature,
			CarLoadDocumentItem documentItemToEdit,
			out Error error)
		{
			return IsScannedCodeValid(scannedCode, isScannedCodeValid, out error)
				&& IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(orderId, allWaterOrderItems, out error)
				&& IsSingleItemHavingRequiredOrderAndNomenclatureExists(orderId, nomenclatureId, itemsHavingRequiredNomenclature, out error)
				&& IsNotAllProductsHasTrueMarkCode(orderId, nomenclatureId, documentItemToEdit, out error)
				&& IsTrueMarkCodeNotExistAndHasRequiredGtin(trueMarkCode, documentItemToEdit.Nomenclature.Gtin, scannedCode, out error)
				&& IsTrueMarkCodeIntroduced(trueMarkCode, out error);
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
			IList<CarLoadDocumentItem> allWaterOrderItems,
			IEnumerable<CarLoadDocumentItem> itemsHavingRequiredNomenclature,
			CarLoadDocumentItem documentItemToEdit,
			out Error error)
		{
			return IsScannedCodeValid(oldScannedCode, isOldScannedCodeValid, out error)
				&& IsScannedCodeValid(newScannedCode, isNewScannedCodeValid, out error)
				&& IsTrueMarkCodesHasEqualGtins(oldTrueMarkCode, newTrueMarkCode, out error)
				&& IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(orderId, allWaterOrderItems, out error)
				&& IsSingleItemHavingRequiredOrderAndNomenclatureExists(orderId, nomenclatureId, itemsHavingRequiredNomenclature, out error)
				&& IsProductsHavingRequiredTrueMarkCodeExists(documentItemToEdit, oldTrueMarkCode, out error)
				&& IsTrueMarkCodeNotExists(newTrueMarkCode, newScannedCode, out error)
				&& IsTrueMarkCodeIntroduced(newTrueMarkCode, out error);
		}

		private bool IsSingleItemHavingRequiredOrderAndNomenclatureExists(
			int orderId,
			int nomenclatureId,
			IEnumerable<CarLoadDocumentItem> documentNomenclatureOrderItems,
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
			CarLoadDocumentItem carLoadDocumentItem,
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
			CarLoadDocumentItem carLoadDocumentItem,
			TrueMarkWaterCode trueMarkCode,
			out Error error)
		{
			error = null;

			if(!carLoadDocumentItem
				.TrueMarkCodes.Select(x => x.TrueMarkCode)
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
			var existingDuplicatedCodes =
				_trueMarkRepository.GetTrueMarkCodeDuplicates(_uow, trueMarkCode.GTIN, trueMarkCode.SerialNumber, trueMarkCode.CheckCode).ToList();

			if(existingDuplicatedCodes.Count > 0)
			{
				error = TrueMarkCodeErrors.CreateTrueMarkCodeIsAlreadyExists(scannedCode);
				LogError(error);
				return false;
			}

			return true;
		}

		private bool IsTrueMarkCodeIntroduced(TrueMarkWaterCode trueMarkCode, out Error error)
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
					_trueMarkCodesChecker.CheckCodesAsync(new List<TrueMarkWaterIdentificationCode> { waterCode }, CancellationToken.None)
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
				_logger.LogError(error.Message, ex);
				return false;
			}
		}

		private void LogError(Error error)
		{
			_logger.LogError(error.Message);
		}
	}
}
