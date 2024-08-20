using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Documents;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Errors;
using WarehouseApi.Contracts.Dto;
using WarehouseApi.Contracts.Responses;
using CarLoadDocumentErrors = Vodovoz.Errors.Store.CarLoadDocument;

namespace WarehouseApi.Library.Services
{
	public class CarLoadService : ICarLoadService
	{
		private readonly ILogger<CarLoadService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ICarLoadDocumentRepository _carLoadDocumentRepository;

		public CarLoadService(
			ILogger<CarLoadService> logger,
			IUnitOfWork uow,
			ICarLoadDocumentRepository carLoadDocumentRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_carLoadDocumentRepository = carLoadDocumentRepository ?? throw new ArgumentNullException(nameof(carLoadDocumentRepository));
		}

		public Result<StartLoadResponse> StartLoad(int documentId)
		{
			var carLoadDocument = _carLoadDocumentRepository.GetCarLoadDocumentById(_uow, documentId);

			if(carLoadDocument is null)
			{
				return Result.Failure<StartLoadResponse>(CarLoadDocumentErrors.CreateNotFound(documentId));
			}

			if(carLoadDocument.LoadOperationState == CarLoadDocumentLoadOperationState.InProgress)
			{
				return Result.Failure<StartLoadResponse>(CarLoadDocumentErrors.CreateLoadingIsAlreadyDone(documentId));
			}

			if(carLoadDocument.LoadOperationState == CarLoadDocumentLoadOperationState.Done)
			{
				return Result.Failure<StartLoadResponse>(CarLoadDocumentErrors.CreateLoadingIsAlreadyDone(documentId));
			}

			var response = new StartLoadResponse();
			response.Result = OperationResultEnumDto.Success;
			response.CarLoadDocument = new CarLoadDocumentDto
			{
				Id = documentId,
				Driver = "Super Driver",
				Car = "Super Car",
				LoadPriority = 12,
				State = LoadOperationStateEnumDto.InProgress
			};

			return Result.Success(response);
		}
	}
}
