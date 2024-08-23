using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Documents;
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

		public CarLoadDocumentProcessingErrorsChecker(
			ILogger<CarLoadDocumentProcessingErrorsChecker> logger,
			IUnitOfWork uow,
			ITrueMarkRepository trueMarkRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
		}

		public bool IsCarLoadDocumentLoadOperationStateCanBeSetInProgress(CarLoadDocument carLoadDocument, int documentId, out Error error)
		{
			error = null;

			if(carLoadDocument is null)
			{
				error = CarLoadDocumentErrors.CreateDocumentNotFound(documentId);
				LogError(error);
				return false;
			}

			if(carLoadDocument.LoadOperationState == CarLoadDocumentLoadOperationState.InProgress)
			{
				error = CarLoadDocumentErrors.CreateLoadingIsAlreadyInProgress(documentId);
				LogError(error);
				return false;
			}

			if(carLoadDocument.LoadOperationState == CarLoadDocumentLoadOperationState.Done)
			{
				error = CarLoadDocumentErrors.CreateLoadingIsAlreadyDone(documentId);
				LogError(error);
				return false;
			}

			return true;
		}

		public bool IsDocumentOrderDataCorrect(int orderId, IList<CarLoadDocumentItem> documentOrderItems, out Error error)
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

		public bool IsDocumentItemsNomenclatureDataCorrect(
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

		public bool IsNeedToAddTrueMarkCodesInDocumentItem(
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

		public bool IsScannedCodeValid(string scannedCode, bool isScannedCodeValid, out Error error)
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

		public bool IsTrueMarkCodeCanBeAdded(
			TrueMarkWaterCode trueMarkCode,
			string nomenclatureGtin,
			string scannedCode,
			out Error error)
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

			if(trueMarkCode.GTIN != nomenclatureGtin)
			{
				error = TrueMarkCodeErrors.CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin(scannedCode);
				LogError(error);
				return false;
			}

			return true;
		}

		private void LogError(Error error)
		{
			_logger.LogError(error.Message);
		}
	}
}
