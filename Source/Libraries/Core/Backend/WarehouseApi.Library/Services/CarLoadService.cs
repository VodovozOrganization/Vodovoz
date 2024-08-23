using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Errors;
using Vodovoz.Models;
using Vodovoz.Models.TrueMark;
using VodovozBusiness.Domain.TrueMark;
using WarehouseApi.Contracts.Dto;
using WarehouseApi.Contracts.Responses;
using WarehouseApi.Library.Converters;
using WarehouseApi.Library.Errors;

namespace WarehouseApi.Library.Services
{
	public class CarLoadService : ICarLoadService
	{
		private readonly ILogger<CarLoadService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ICarLoadDocumentRepository _carLoadDocumentRepository;
		private readonly IRouteListDailyNumberProvider _routeListDailyNumberProvider;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly CarLoadDocumentConverter _carLoadDocumentConverter;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly CarLoadDocumentProcessingErrorsChecker _documentErrorsChecker;

		public CarLoadService(
			ILogger<CarLoadService> logger,
			IUnitOfWork uow,
			ICarLoadDocumentRepository carLoadDocumentRepository,
			IRouteListDailyNumberProvider routeListDailyNumberProvider,
			ITrueMarkRepository trueMarkRepository,
			CarLoadDocumentConverter carLoadDocumentConverter,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			CarLoadDocumentProcessingErrorsChecker documentErrorsChecker)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_carLoadDocumentRepository = carLoadDocumentRepository ?? throw new ArgumentNullException(nameof(carLoadDocumentRepository));
			_routeListDailyNumberProvider = routeListDailyNumberProvider ?? throw new ArgumentNullException(nameof(routeListDailyNumberProvider));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_carLoadDocumentConverter = carLoadDocumentConverter ?? throw new ArgumentNullException(nameof(carLoadDocumentConverter));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_documentErrorsChecker = documentErrorsChecker ?? throw new ArgumentNullException(nameof(documentErrorsChecker));
		}

		public async Task<Result<StartLoadResponse>> StartLoad(int documentId)
		{
			var response = new StartLoadResponse();

			_logger.LogInformation("Получаем данные по талону погрузки #{DocumentId}", documentId);
			var carLoadDocument =
				(await _carLoadDocumentRepository.GetCarLoadDocumentsById(_uow, documentId).ToListAsync())
				.FirstOrDefault();

			if(!_documentErrorsChecker.IsCarLoadDocumentLoadOperationStateCanBeSetInProgress(carLoadDocument, documentId, out Error error))
			{
				response.CarLoadDocument = carLoadDocument is null ? null : GetCarLoadDocumentDto(carLoadDocument);
				response.Result = OperationResultEnumDto.Error;
				response.Error = error.Message;

				return Result.Failure(response, error);
			}

			SetLoadOperationStateAndSaveDocument(carLoadDocument, CarLoadDocumentLoadOperationState.InProgress);

			response.CarLoadDocument = carLoadDocument is null ? null : GetCarLoadDocumentDto(carLoadDocument);
			response.Result = OperationResultEnumDto.Success;
			response.Error = null;

			return Result.Success(response);
		}

		public async Task<Result<GetOrderResponse>> GetOrder(int orderId)
		{
			var documentOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);

			var response = new GetOrderResponse
			{
				Order = _carLoadDocumentConverter.ConvertToApiOrder(documentOrderItems)
			};

			if(!_documentErrorsChecker.IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(orderId, documentOrderItems, out Error error))
			{
				response.Result = OperationResultEnumDto.Error;
				response.Error = error.Message;

				return Result.Failure(response, error);
			}

			response.Result = OperationResultEnumDto.Success;
			response.Error = null;

			return Result.Success(response);
		}

		public async Task<Result<AddOrderCodeResponse>> AddOrderCode(int orderId, int nomenclatureId, string scannedCode)
		{
			var response = new AddOrderCodeResponse();

			var isScannedCodeValid = _trueMarkWaterCodeParser.TryParse(scannedCode, out TrueMarkWaterCode trueMarkCode);

			var allWaterOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var itemsHavingRequiredNomenclature = allWaterOrderItems.Where(item => item.Nomenclature.Id == nomenclatureId).ToList();
			var documentItemToEdit = itemsHavingRequiredNomenclature.FirstOrDefault();

			if(!_documentErrorsChecker.IsTrueMarkCodeCanBeAdded(
				orderId,
				nomenclatureId,
				scannedCode,
				isScannedCodeValid,
				trueMarkCode,
				allWaterOrderItems,
				itemsHavingRequiredNomenclature,
				documentItemToEdit,
				out Error error))
			{
				response.Nomenclature = documentItemToEdit is null ? null : _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit);
				response.Result = OperationResultEnumDto.Error;
				response.Error = error.Message;

				return Result.Failure(response, error);
			}

			AddTrueMarkCodeAndSaveCarLoadDocumentItem(nomenclatureId, documentItemToEdit, trueMarkCode);

			response.Nomenclature = documentItemToEdit is null ? null : _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit);
			response.Result = OperationResultEnumDto.Success;
			response.Error = null;

			return Result.Success(response);
		}

		public async Task<Result<AddOrderCodeResponse>> ChangeOrderCode(int orderId, int nomenclatureId, string oldScannedCode, string newScannedCode)
		{
			var response = new AddOrderCodeResponse();

			var isOldScannedCodeValid = _trueMarkWaterCodeParser.TryParse(oldScannedCode, out TrueMarkWaterCode oldTrueMarkCode);
			var isNewScannedCodeValid = _trueMarkWaterCodeParser.TryParse(newScannedCode, out TrueMarkWaterCode newTrueMarkCode);

			var allWaterOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var itemsHavingRequiredNomenclature = allWaterOrderItems.Where(item => item.Nomenclature.Id == nomenclatureId).ToList();
			var documentItemToEdit = itemsHavingRequiredNomenclature.FirstOrDefault();

			if(!_documentErrorsChecker.IsTrueMarkCodeCanBeChanged(
				orderId,
				nomenclatureId,
				oldScannedCode,
				isOldScannedCodeValid,
				oldTrueMarkCode,
				newScannedCode,
				isNewScannedCodeValid,
				newTrueMarkCode,
				allWaterOrderItems,
				itemsHavingRequiredNomenclature,
				documentItemToEdit,
				out Error error))
			{
				response.Nomenclature = documentItemToEdit is null ? null : _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit);
				response.Result = OperationResultEnumDto.Error;
				response.Error = error.Message;

				return Result.Failure(response, error);
			}

			ChangeTrueMarkCodeAndSaveCarLoadDocumentItem(documentItemToEdit, oldTrueMarkCode, newTrueMarkCode);

			response.Nomenclature = documentItemToEdit is null ? null : _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit);
			response.Result = OperationResultEnumDto.Success;
			response.Error = null;

			return Result.Success(response);
		}

		public async Task<Result<EndLoadResponse>> EndLoad(int documentId)
		{
			var response = new EndLoadResponse();

			_logger.LogInformation("Получаем данные по талону погрузки #{DocumentId}", documentId);
			var carLoadDocument =
				(await _carLoadDocumentRepository.GetCarLoadDocumentsById(_uow, documentId).ToListAsync())
				.FirstOrDefault();

			if(!_documentErrorsChecker.IsCarLoadDocumentLoadOperationStateCanBeSetInDone(carLoadDocument, documentId, out Error error))
			{
				response.Result = OperationResultEnumDto.Error;
				response.Error = error.Message;

				return Result.Failure(response, error);
			}

			SetLoadOperationStateAndSaveDocument(carLoadDocument, CarLoadDocumentLoadOperationState.Done);

			response.Result = OperationResultEnumDto.Success;
			response.Error = null;

			return Result.Success(response);
		}

		private async Task<IList<CarLoadDocumentItem>> GetCarLoadDocumentWaterOrderItems(int orderId)
		{
			_logger.LogInformation("Получаем данные по заказу #{OrderId} из талона погрузки", orderId);
			var documentOrderItems = await _carLoadDocumentRepository.GetWaterItemsInCarLoadDocumentById(_uow, orderId).ToListAsync();

			return documentOrderItems;
		}

		private CarLoadDocumentDto GetCarLoadDocumentDto(CarLoadDocument carLoadDocument)
		{
			var loadPriority =
				_routeListDailyNumberProvider.GetOrCreateDailyNumber(carLoadDocument.RouteList.Id, carLoadDocument.RouteList.Date);

			var carLoadDocumentDto =
				_carLoadDocumentConverter.ConvertToApiCarLoadDocument(carLoadDocument, loadPriority);

			return carLoadDocumentDto;
		}

		private void SetLoadOperationStateAndSaveDocument(CarLoadDocument document, CarLoadDocumentLoadOperationState newLoadOperationState)
		{
			var currentLoadOperationState = document.LoadOperationState;

			_logger.LogInformation("Меняем статус талона погрузки #{DocumentId} с \"{CurrentStatus}\" на \"{NewStatus}\"",
				document.Id,
				currentLoadOperationState.GetEnumTitle(),
				newLoadOperationState.GetEnumTitle());

			document.LoadOperationState = newLoadOperationState;
			_uow.Save(document);
			_uow.Commit();

			_logger.LogInformation("Статус талона погрузки #{DocumentId} успешно изменен на \"{NewStatus}\"",
				document.Id,
				newLoadOperationState.GetEnumTitle());
		}

		private void AddTrueMarkCodeAndSaveCarLoadDocumentItem(int nomenclatureId, CarLoadDocumentItem carLoadDocumentItem, TrueMarkWaterCode trueMarkCode)
		{
			var codeEntity = CreateTrueMarkCodeEntity(trueMarkCode);

			carLoadDocumentItem.TrueMarkCodes.Add(new CarLoadDocumentItemTrueMarkCode
			{
				CarLoadDocumentItem = carLoadDocumentItem,
				SequenceNumber = carLoadDocumentItem.TrueMarkCodes.Count,
				TrueMarkCode = codeEntity,
				NomenclatureId = nomenclatureId
			});

			_uow.Save(codeEntity);
			_uow.Save(carLoadDocumentItem);
			_uow.Commit();
		}

		private void ChangeTrueMarkCodeAndSaveCarLoadDocumentItem(CarLoadDocumentItem carLoadDocumentItem, TrueMarkWaterCode oldTrueMarkCode, TrueMarkWaterCode newTrueMarkCode)
		{
			var codeToEdit = carLoadDocumentItem.TrueMarkCodes
				.Select(x => x.TrueMarkCode)
				.Where(x => x.GTIN == oldTrueMarkCode.GTIN && x.SerialNumber == oldTrueMarkCode.SerialNumber && x.CheckCode == oldTrueMarkCode.CheckCode)
				.First();

			codeToEdit.RawCode = newTrueMarkCode.SourceCode;
			codeToEdit.GTIN = newTrueMarkCode.GTIN;
			codeToEdit.SerialNumber = newTrueMarkCode.SerialNumber;
			codeToEdit.CheckCode = newTrueMarkCode.CheckCode;
			codeToEdit.IsInvalid = false;

			_uow.Save(codeToEdit);
			_uow.Save(carLoadDocumentItem);
			_uow.Commit();
		}

		private TrueMarkWaterIdentificationCode CreateTrueMarkCodeEntity(TrueMarkWaterCode trueMarkCode)
		{
			var codeEntity = new TrueMarkWaterIdentificationCode
			{
				IsInvalid = false,
				RawCode = trueMarkCode.SourceCode.Substring(0, Math.Min(255, trueMarkCode.SourceCode.Length)),
				GTIN = trueMarkCode.GTIN,
				SerialNumber = trueMarkCode.SerialNumber,
				CheckCode = trueMarkCode.CheckCode
			};

			return codeEntity;
		}
	}
}
