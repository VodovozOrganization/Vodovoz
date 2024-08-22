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
using CarLoadDocumentErrors = Vodovoz.Errors.Store.CarLoadDocument;

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

		public CarLoadService(
			ILogger<CarLoadService> logger,
			IUnitOfWork uow,
			ICarLoadDocumentRepository carLoadDocumentRepository,
			IRouteListDailyNumberProvider routeListDailyNumberProvider,
			ITrueMarkRepository trueMarkRepository,
			CarLoadDocumentConverter carLoadDocumentConverter,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_carLoadDocumentRepository = carLoadDocumentRepository ?? throw new ArgumentNullException(nameof(carLoadDocumentRepository));
			_routeListDailyNumberProvider = routeListDailyNumberProvider ?? throw new ArgumentNullException(nameof(routeListDailyNumberProvider));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_carLoadDocumentConverter = carLoadDocumentConverter ?? throw new ArgumentNullException(nameof(carLoadDocumentConverter));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
		}

		public async Task<Result<StartLoadResponse>> StartLoad(int documentId)
		{
			Error error = null;
			var response = new StartLoadResponse();

			_logger.LogInformation("Получаем данные по талону погрузки #{DocumentId}", documentId);
			var carLoadDocument =
				(await _carLoadDocumentRepository.GetCarLoadDocumentsById(_uow, documentId).ToListAsync())
				.FirstOrDefault();

			if(carLoadDocument is null)
			{
				_logger.LogInformation("Талон погрузки #{DocumentId} не найден", documentId);
				error = CarLoadDocumentErrors.CreateDocumentNotFound(documentId);
			}
			else if(carLoadDocument.LoadOperationState == CarLoadDocumentLoadOperationState.InProgress)
			{
				_logger.LogInformation("Талон погрузки #{DocumentId} уже в процессе погрузки", documentId);
				error = CarLoadDocumentErrors.CreateLoadingIsAlreadyInProgress(documentId);
			}
			else if(carLoadDocument.LoadOperationState == CarLoadDocumentLoadOperationState.Done)
			{
				_logger.LogInformation("Талон погрузки #{DocumentId} уже погружен", documentId);
				error = CarLoadDocumentErrors.CreateLoadingIsAlreadyDone(documentId);
			}

			if(carLoadDocument != null)
			{
				response.CarLoadDocument = GetCarLoadDocumentDto(carLoadDocument);
			}

			if(error != null)
			{
				response.Result = OperationResultEnumDto.Error;
				response.Error = error.Message;

				return Result.Failure(response, error);
			}

			SetLoadOperationStateInProgressAndSaveDocument(carLoadDocument);

			response.Result = OperationResultEnumDto.Success;
			response.Error = null;

			return Result.Success(response);
		}

		public async Task<Result<GetOrderResponse>> GetOrder(int orderId)
		{
			var documentOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var errors = GetOrderErrors(orderId, documentOrderItems);
			var response = new GetOrderResponse();

			if(errors.Count > 0)
			{
				var firstError = errors.First();

				response.Result = OperationResultEnumDto.Error;
				response.Error = firstError.Message;

				return Result.Failure(response, firstError);
			}

			response.Order = _carLoadDocumentConverter.ConvertToApiOrder(documentOrderItems);

			response.Result = OperationResultEnumDto.Success;
			response.Error = null;

			return Result.Success(response);
		}

		public async Task<Result<AddOrderCodeResponse>> AddOrderCode(int orderId, int nomenclatureId, string code)
		{
			CarLoadDocumentItem carLoadDocumentItem = null;
			TrueMarkWaterCode trueMarkCode = null;

			var documentOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var documentOrderNomenclatureItems = documentOrderItems.Where(item => item.Nomenclature.Id == nomenclatureId).ToList();

			var errors = new List<Error>();
			errors.AddRange(GetOrderErrors(orderId, documentOrderItems));
			errors.AddRange(GetOrderNomenclatureErrors(orderId, nomenclatureId, documentOrderNomenclatureItems));

			if(errors.Count == 0)
			{
				bool isValidCode = _trueMarkWaterCodeParser.TryParse(code, out trueMarkCode);

				if(isValidCode)
				{
					carLoadDocumentItem = documentOrderNomenclatureItems.First();
					errors.AddRange(await GetTrueMarkCodesErrors(trueMarkCode, carLoadDocumentItem.Nomenclature.Gtin, code));
				}
				else
				{
					_logger.LogInformation("Полученная строка кода ЧЗ \"{CodeString}\" невалидна", code);
					errors.Add(CarLoadDocumentErrors.CreateTrueMarkCodeStringIsNotValid(code));
				}
			}

			var response = new AddOrderCodeResponse();

			if(errors.Count > 0)
			{
				var firstError = errors.First();

				response.Result = OperationResultEnumDto.Error;
				response.Error = firstError.Message;
				response.Nomenclature =
					carLoadDocumentItem is null
					? null
					: _carLoadDocumentConverter.ConvertToApiNomenclature(carLoadDocumentItem);

				return Result.Failure(response, firstError);
			}

			AddTrueMarkCodeAndSaveCarLoadDocumentItem(nomenclatureId, carLoadDocumentItem, trueMarkCode);

			response.Nomenclature = _carLoadDocumentConverter.ConvertToApiNomenclature(carLoadDocumentItem);

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

		private IList<Error> GetOrderErrors(int orderId, IList<CarLoadDocumentItem> documentOrderItems)
		{
			var errors = new List<Error>();
			Error error = null;

			if(documentOrderItems is null || documentOrderItems.Count == 0)
			{
				_logger.LogInformation("Данные заказа #{OrderId} в талонах погрузки не найдены", orderId);
				error = CarLoadDocumentErrors.CreateOrderNotFound(orderId);
			}
			else if(documentOrderItems.Select(oi => oi.Document.Id).Distinct().Count() > 1)
			{
				_logger.LogInformation("Строки заказа #{OrderId} сетевого клиента присутствуют в нескольких талонах погрузки", orderId);
				error = CarLoadDocumentErrors.CreateOrderItemsExistInMultipleDocuments(orderId);
			}

			if(error != null)
			{
				errors.Add(error);
			}

			return errors;
		}

		private IList<Error> GetOrderNomenclatureErrors(int orderId, int nomenclatureId, IList<CarLoadDocumentItem> documentNomenclatureOrderItems)
		{
			var errors = new List<Error>();
			Error error = null;

			if(documentNomenclatureOrderItems.Count == 0)
			{
				_logger.LogInformation("В сетевом заказе #{OrderId} номенклатура #{NomenclatureId} не найдена", orderId, nomenclatureId);
				error = CarLoadDocumentErrors.CreateOrderDoesNotContainNomenclature(orderId, nomenclatureId);
			}
			else if(documentNomenclatureOrderItems.Count > 1)
			{
				_logger.LogInformation("В талоне погрузки имеется несколько строк сетевого заказа #{OrderId} с номенклатурой #{NomenclatureId}", orderId, nomenclatureId);
				error = CarLoadDocumentErrors.CreateOrderNomenclatureExistInMultipleDocumentItems(orderId, nomenclatureId);
			}

			if(error != null)
			{
				errors.Add(error);
			}

			return errors;
		}

		private async Task<IList<Error>> GetTrueMarkCodesErrors(TrueMarkWaterCode trueMarkCode, string nomenclatureGtin, string rawCodeString)
		{
			var errors = new List<Error>();

			var existingDuplicatedCodes =
				await _trueMarkRepository.GetTrueMarkCodeDuplicates(_uow, trueMarkCode.GTIN, trueMarkCode.SerialNumber, trueMarkCode.CheckCode).ToListAsync();

			if(existingDuplicatedCodes.Count > 0)
			{
				_logger.LogInformation("Код ЧЗ \"{CodeString}\" уже имеется в базе. Добавляемый код является дублем", rawCodeString);
				errors.Add(CarLoadDocumentErrors.CreateTrueMarkCodeIsAlreadyExists(rawCodeString));

				return errors;
			}

			if(trueMarkCode.GTIN != nomenclatureGtin)
			{
				_logger.LogInformation("Значение GTIN переданного кода \"{CodeString}\" не соответствует значению GTIN для указанной номенклатуры", rawCodeString);
				errors.Add(CarLoadDocumentErrors.CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin(rawCodeString));

				return errors;
			}

			return errors;
		}

		private CarLoadDocumentDto GetCarLoadDocumentDto(CarLoadDocument carLoadDocument)
		{
			var loadPriority =
				_routeListDailyNumberProvider.GetOrCreateDailyNumber(carLoadDocument.RouteList.Id, carLoadDocument.RouteList.Date);

			var carLoadDocumentDto =
				_carLoadDocumentConverter.ConvertToApiCarLoadDocument(carLoadDocument, loadPriority);

			return carLoadDocumentDto;
		}

		private void SetLoadOperationStateInProgressAndSaveDocument(CarLoadDocument document)
		{
			var currentLoadOperationState = document.LoadOperationState;
			var newLoadOperationState = CarLoadDocumentLoadOperationState.InProgress;

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
			var codeEntity = new TrueMarkWaterIdentificationCode
			{
				IsInvalid = true,
				RawCode = trueMarkCode.SourceCode.Substring(0, Math.Min(255, trueMarkCode.SourceCode.Length)),
				GTIN = trueMarkCode.GTIN,
				SerialNumber = trueMarkCode.SerialNumber,
				CheckCode = trueMarkCode.CheckCode
			};

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
	}
}
