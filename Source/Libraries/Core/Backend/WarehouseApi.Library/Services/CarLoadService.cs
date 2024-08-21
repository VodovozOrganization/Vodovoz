using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Documents;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Errors;
using Vodovoz.Models;
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
		private readonly CarLoadDocumentConverter _carLoadDocumentConverter;

		public CarLoadService(
			ILogger<CarLoadService> logger,
			IUnitOfWork uow,
			ICarLoadDocumentRepository carLoadDocumentRepository,
			IRouteListDailyNumberProvider routeListDailyNumberProvider,
			CarLoadDocumentConverter carLoadDocumentConverter)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_carLoadDocumentRepository = carLoadDocumentRepository ?? throw new ArgumentNullException(nameof(carLoadDocumentRepository));
			_routeListDailyNumberProvider = routeListDailyNumberProvider ?? throw new ArgumentNullException(nameof(routeListDailyNumberProvider));
			_carLoadDocumentConverter = carLoadDocumentConverter ?? throw new ArgumentNullException(nameof(carLoadDocumentConverter));
		}

		public Result<StartLoadResponse> StartLoad(int documentId)
		{
			Error error = null;
			var response = new StartLoadResponse();

			_logger.LogInformation("Получаем данные по талону погрузки #{DocumentId}", documentId);
			var carLoadDocument = _carLoadDocumentRepository.GetCarLoadDocumentById(_uow, documentId);

			if(carLoadDocument is null)
			{
				_logger.LogInformation("Талон погрузки #{DocumentId} не найден", documentId);
				error = CarLoadDocumentErrors.CreateNotFound(documentId);
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

			SetLoadOperationStateInProgress(carLoadDocument);

			response.Result = OperationResultEnumDto.Success;
			response.Error = null;

			return Result.Success(response);
		}

		private CarLoadDocumentDto GetCarLoadDocumentDto(CarLoadDocument carLoadDocument)
		{
			var loadPriority =
				_routeListDailyNumberProvider.GetOrCreateDailyNumber(carLoadDocument.RouteList.Id, carLoadDocument.RouteList.Date);

			var carLoadDocumentDto =
				_carLoadDocumentConverter.ConvertToApiCarLoadDocument(carLoadDocument, loadPriority);

			return carLoadDocumentDto;
		}

		private void SetLoadOperationStateInProgress(CarLoadDocument document)
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
	}
}
