using Edo.Transport.Messages.Events;
using Gamma.Utilities;
using MassTransit;
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
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Errors;
using Vodovoz.Models;
using VodovozBusiness.Services.TrueMark;
using WarehouseApi.Contracts.Dto;
using WarehouseApi.Contracts.Responses;
using WarehouseApi.Library.Common;
using WarehouseApi.Library.Converters;
using WarehouseApi.Library.Errors;
using CarLoadDocumentErrors = Vodovoz.Errors.Stores.CarLoadDocument;

namespace WarehouseApi.Library.Services
{
	public class CarLoadService : ICarLoadService
	{
		private readonly ILogger<CarLoadService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ICarLoadDocumentRepository _carLoadDocumentRepository;
		private readonly IEmployeeWithLoginRepository _employeeWithLoginRepository;
		private readonly IRouteListDailyNumberProvider _routeListDailyNumberProvider;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly ILogisticsEventsCreationService _logisticsEventsCreationService;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly CarLoadDocumentConverter _carLoadDocumentConverter;
		private readonly CarLoadDocumentProcessingErrorsChecker _documentErrorsChecker;
		private readonly IBus _messageBus;

		public CarLoadService(
			ILogger<CarLoadService> logger,
			IUnitOfWork uow,
			ICarLoadDocumentRepository carLoadDocumentRepository,
			IEmployeeWithLoginRepository employeeWithLoginRepository,
			IRouteListDailyNumberProvider routeListDailyNumberProvider,
			ITrueMarkRepository trueMarkRepository,
			ILogisticsEventsCreationService logisticsEventsCreationService,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			CarLoadDocumentConverter carLoadDocumentConverter,
			CarLoadDocumentProcessingErrorsChecker documentErrorsChecker,
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_carLoadDocumentRepository = carLoadDocumentRepository ?? throw new ArgumentNullException(nameof(carLoadDocumentRepository));
			_employeeWithLoginRepository = employeeWithLoginRepository;
			_routeListDailyNumberProvider = routeListDailyNumberProvider ?? throw new ArgumentNullException(nameof(routeListDailyNumberProvider));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_logisticsEventsCreationService = logisticsEventsCreationService ?? throw new ArgumentNullException(nameof(logisticsEventsCreationService));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_carLoadDocumentConverter = carLoadDocumentConverter ?? throw new ArgumentNullException(nameof(carLoadDocumentConverter));
			_documentErrorsChecker = documentErrorsChecker ?? throw new ArgumentNullException(nameof(documentErrorsChecker));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task<RequestProcessingResult<StartLoadResponse>> StartLoad(int documentId, string userLogin, string accessToken, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем данные по талону погрузки #{DocumentId}", documentId);
			var carLoadDocument =
				(await _carLoadDocumentRepository.GetCarLoadDocumentsById(_uow, documentId))
				.FirstOrDefault();

			var pickerEmployee = GetEmployeeProxyByApiLogin(userLogin);

			var failureResponse = new StartLoadResponse
			{
				CarLoadDocument = carLoadDocument is null ? null : GetCarLoadDocumentDto(carLoadDocument),
				Result = OperationResultEnumDto.Error
			};

			var checkResult = _documentErrorsChecker.IsEmployeeCanPickUpCarLoadDocument(documentId, pickerEmployee);

			if(checkResult.IsFailure)
			{
				var error = checkResult.Errors.FirstOrDefault();

				failureResponse.Error = error.Message;

				var result = Result.Failure<StartLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			CreateAndSaveCarLoadDocumentLoadingProcessAction(carLoadDocument?.Id ?? 0, pickerEmployee, CarLoadDocumentLoadingProcessActionType.StartLoad);

			checkResult = _documentErrorsChecker.IsCarLoadDocumentLoadingCanBeStarted(carLoadDocument, documentId);

			if(checkResult.IsFailure)
			{
				var error = checkResult.Errors.FirstOrDefault();

				failureResponse.Error = error.Message;

				var result = Result.Failure<StartLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			var isDocumentLoadOperationStateUpdated =
				SetDocumentLoadOperationState(carLoadDocument, CarLoadDocumentLoadOperationState.InProgress);

			if(!isDocumentLoadOperationStateUpdated)
			{
				var error = CarLoadDocumentErrors.CreateCarLoadDocumentStateChangeError(carLoadDocument.Id);
				failureResponse.Error = error.Message;

				var result = Result.Failure<StartLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			var isLogisticEventCreated =
				await CreateStartLoadLogisticEvent(carLoadDocument.Id, accessToken, cancellationToken);

			if(!isLogisticEventCreated)
			{
				var error = CarLoadDocumentErrors.CarLoadDocumentLogisticEventCreationError;
				failureResponse.Error = error.Message;

				var result = Result.Failure<StartLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			_uow.Commit();

			var successResponse = new StartLoadResponse
			{
				CarLoadDocument = carLoadDocument is null ? null : GetCarLoadDocumentDto(carLoadDocument),
				Result = OperationResultEnumDto.Success,
				Error = null
			};

			return RequestProcessingResult.CreateSuccess(Result.Success(successResponse));
		}

		public async Task<RequestProcessingResult<GetOrderResponse>> GetOrder(int orderId)
		{
			var documentOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);

			var response = new GetOrderResponse
			{
				Order = _carLoadDocumentConverter.ConvertToApiOrder(documentOrderItems)
			};

			var checkResult = _documentErrorsChecker.IsOrderNeedIndividualSetOnLoad(orderId);

			if(checkResult.IsFailure)
			{
				var error = checkResult.Errors.FirstOrDefault();

				response.Result = OperationResultEnumDto.Error;
				response.Error = error.Message;

				var result = Result.Failure<GetOrderResponse>(error);

				return RequestProcessingResult.CreateFailure(result, response);
			}

			checkResult = _documentErrorsChecker.IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(orderId, documentOrderItems);

			if(checkResult.IsFailure)
			{
				var error = checkResult.Errors.FirstOrDefault();

				response.Result = OperationResultEnumDto.Error;
				response.Error = error.Message;

				var result = Result.Failure<GetOrderResponse>(error);

				return RequestProcessingResult.CreateFailure(result, response);
			}

			response.Result = OperationResultEnumDto.Success;
			response.Error = null;

			return RequestProcessingResult.CreateSuccess(Result.Success(response));
		}

		public async Task<RequestProcessingResult<AddOrderCodeResponse>> AddOrderCode(
			int orderId,
			int nomenclatureId,
			string scannedCode,
			string userLogin,
			CancellationToken cancellationToken)
		{
			var trueMarkWaterCode = _trueMarkWaterCodeService.LoadOrCreateTrueMarkWaterIdentificationCode(_uow, scannedCode);

			var allWaterOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var itemsHavingRequiredNomenclature = allWaterOrderItems.Where(item => item.Nomenclature.Id == nomenclatureId).ToList();
			var documentItemToEdit = itemsHavingRequiredNomenclature.FirstOrDefault();

			var pickerEmployee = GetEmployeeProxyByApiLogin(userLogin);

			var failureResponse = new AddOrderCodeResponse
			{
				Nomenclature = documentItemToEdit is null ? null : _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit),
				Result = OperationResultEnumDto.Error,
			};

			var checkResult = _documentErrorsChecker.IsEmployeeCanPickUpCarLoadDocument(documentItemToEdit?.Document?.Id ?? 0, pickerEmployee);

			if(checkResult.IsFailure)
			{
				var error = checkResult.Errors.FirstOrDefault();

				failureResponse.Error = error.Message;

				var result = Result.Failure<AddOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			CreateAndSaveCarLoadDocumentLoadingProcessAction(
				documentItemToEdit?.Document?.Id ?? 0,
				pickerEmployee,
				CarLoadDocumentLoadingProcessActionType.AddTrueMarkCode);

			checkResult = await _documentErrorsChecker.IsTrueMarkCodeCanBeAdded(
				orderId,
				nomenclatureId,
				trueMarkWaterCode,
				allWaterOrderItems,
				itemsHavingRequiredNomenclature,
				documentItemToEdit,
				cancellationToken);

			if(checkResult.IsFailure)
			{
				var error = checkResult.Errors.FirstOrDefault();

				failureResponse.Error = error.Message;

				var result = Result.Failure<AddOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			AddTrueMarkCodeToDocumentItem(documentItemToEdit, trueMarkWaterCode);

			_uow.Save(documentItemToEdit);
			_uow.Commit();

			var successResponse = new AddOrderCodeResponse
			{
				Nomenclature = _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit),
				Result = OperationResultEnumDto.Success,
				Error = null
			};

			return RequestProcessingResult.CreateSuccess(Result.Success(successResponse));
		}

		public async Task<RequestProcessingResult<ChangeOrderCodeResponse>> ChangeOrderCode(
			int orderId,
			int nomenclatureId,
			string oldScannedCode,
			string newScannedCode,
			string userLogin,
			CancellationToken cancellationToken)
		{
			var oldTrueMarkWaterCode = _trueMarkWaterCodeService.LoadOrCreateTrueMarkWaterIdentificationCode(_uow, oldScannedCode);
			var newTrueMarkWaterCode = _trueMarkWaterCodeService.LoadOrCreateTrueMarkWaterIdentificationCode(_uow, newScannedCode);

			var allWaterOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var itemsHavingRequiredNomenclature = allWaterOrderItems.Where(item => item.Nomenclature.Id == nomenclatureId).ToList();
			var documentItemToEdit = itemsHavingRequiredNomenclature.FirstOrDefault();

			var pickerEmployee = GetEmployeeProxyByApiLogin(userLogin);

			var failureResponse = new ChangeOrderCodeResponse
			{
				Nomenclature = documentItemToEdit is null ? null : _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit),
				Result = OperationResultEnumDto.Error,
			};

			var checkResult = _documentErrorsChecker.IsEmployeeCanPickUpCarLoadDocument(documentItemToEdit?.Document?.Id ?? 0, pickerEmployee);

			if(checkResult.IsFailure)
			{
				var error = checkResult.Errors.FirstOrDefault();

				failureResponse.Error = error.Message;

				var result = Result.Failure<ChangeOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			CreateAndSaveCarLoadDocumentLoadingProcessAction(
				documentItemToEdit?.Document?.Id ?? 0,
				pickerEmployee,
				CarLoadDocumentLoadingProcessActionType.ChangeTrueMarkCode);

			checkResult = await _documentErrorsChecker.IsTrueMarkCodeCanBeChanged(
				orderId,
				nomenclatureId,
				oldTrueMarkWaterCode,
				newTrueMarkWaterCode,
				allWaterOrderItems,
				itemsHavingRequiredNomenclature,
				documentItemToEdit,
				cancellationToken);

			if(checkResult.IsFailure)
			{
				var error = checkResult.Errors.FirstOrDefault();

				failureResponse.Error = error.Message;

				var result = Result.Failure<ChangeOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			ChangeTrueMarkCodeAndSaveCarLoadDocumentItem(documentItemToEdit, oldTrueMarkWaterCode, newTrueMarkWaterCode);

			var successResponse = new ChangeOrderCodeResponse
			{
				Nomenclature = documentItemToEdit is null ? null : _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit),
				Result = OperationResultEnumDto.Success,
				Error = null
			};

			return RequestProcessingResult.CreateSuccess(Result.Success(successResponse));
		}

		public async Task<RequestProcessingResult<EndLoadResponse>> EndLoad(int documentId, string userLogin, string accessToken, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем данные по талону погрузки #{DocumentId}", documentId);
			var carLoadDocument =
				(await _carLoadDocumentRepository.GetCarLoadDocumentsById(_uow, documentId))
				.FirstOrDefault();

			var pickerEmployee = GetEmployeeProxyByApiLogin(userLogin);

			var failureResponse = new EndLoadResponse
			{
				Result = OperationResultEnumDto.Error
			};

			var checkResult = _documentErrorsChecker.IsEmployeeCanPickUpCarLoadDocument(documentId, pickerEmployee);

			if(checkResult.IsFailure)
			{
				var error = checkResult.Errors.FirstOrDefault();

				failureResponse.Error = error.Message;

				var result = Result.Failure<EndLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			CreateAndSaveCarLoadDocumentLoadingProcessAction(carLoadDocument?.Id ?? 0, pickerEmployee, CarLoadDocumentLoadingProcessActionType.EndLoad);

			checkResult = _documentErrorsChecker.IsCarLoadDocumentLoadingCanBeDone(carLoadDocument, documentId);

			if(checkResult.IsFailure)
			{
				var error = checkResult.Errors.FirstOrDefault();

				failureResponse.Error = error.Message;

				var result = Result.Failure<EndLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			var isDocumentLoadOperationStateUpdated =
				SetDocumentLoadOperationState(carLoadDocument, CarLoadDocumentLoadOperationState.Done);

			if(!isDocumentLoadOperationStateUpdated)
			{
				var error = CarLoadDocumentErrors.CreateCarLoadDocumentStateChangeError(carLoadDocument.Id);

				failureResponse.Error = error.Message;

				var result = Result.Failure<EndLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			var isLogisticEventCreated =
				await CreateEndLoadLogisticEvent(carLoadDocument.Id, accessToken, cancellationToken);

			if(!isLogisticEventCreated)
			{
				var error = CarLoadDocumentErrors.CarLoadDocumentLogisticEventCreationError;
				failureResponse.Error = error.Message;

				var result = Result.Failure<EndLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			var successResponse = new EndLoadResponse
			{
				Result = OperationResultEnumDto.Success,
				Error = null
			};

			var edoRequests = CreateEdoRequests(carLoadDocument);

			await PublishEdoRequestCreatedEvents(edoRequests);

			_uow.Commit();

			return RequestProcessingResult.CreateSuccess(Result.Success(successResponse));
		}

		private async Task<IEnumerable<CarLoadDocumentItemEntity>> GetCarLoadDocumentWaterOrderItems(int orderId)
		{
			_logger.LogInformation("Получаем данные по заказу #{OrderId} из талона погрузки", orderId);
			var documentOrderItems =
				await _carLoadDocumentRepository.GeAccountableInTrueMarkHavingGtinItemsByCarLoadDocumentId(_uow, orderId);

			return documentOrderItems;
		}

		private CarLoadDocumentDto GetCarLoadDocumentDto(CarLoadDocumentEntity carLoadDocument)
		{
			var loadPriority =
				_routeListDailyNumberProvider.GetOrCreateDailyNumber(carLoadDocument.RouteList.Id, carLoadDocument.RouteList.Date);

			var carLoadDocumentDto =
				_carLoadDocumentConverter.ConvertToApiCarLoadDocument(carLoadDocument, loadPriority);

			return carLoadDocumentDto;
		}

		private bool SetDocumentLoadOperationState(
			CarLoadDocumentEntity document,
			CarLoadDocumentLoadOperationState newLoadOperationState)
		{
			if(newLoadOperationState != CarLoadDocumentLoadOperationState.InProgress
				&& newLoadOperationState != CarLoadDocumentLoadOperationState.Done)
			{
				return false;
			}

			var currentLoadOperationState = document.LoadOperationState;

			_logger.LogInformation("Меняем статус талона погрузки #{DocumentId} с \"{CurrentStatus}\" на \"{NewStatus}\"",
				document.Id,
				currentLoadOperationState,
				newLoadOperationState);

			document.LoadOperationState = newLoadOperationState;

			_uow.Save(document);

			_logger.LogInformation("Статус талона погрузки #{DocumentId} успешно изменен на \"{NewStatus}\"",
				document.Id,
				newLoadOperationState.GetEnumTitle());

			return true;
		}

		private void AddTrueMarkCodeToDocumentItem(CarLoadDocumentItemEntity carLoadDocumentItem, TrueMarkWaterIdentificationCode trueMarkWaterCode)
		{
			carLoadDocumentItem.TrueMarkCodes.Add(new CarLoadDocumentItemTrueMarkProductCode
			{
				CreationTime = DateTime.Now,
				SourceCode = trueMarkWaterCode,
				ResultCode = trueMarkWaterCode,
				Problem = ProductCodeProblem.None,
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				CarLoadDocumentItem = carLoadDocumentItem
			});
		}

		private void ChangeTrueMarkCodeAndSaveCarLoadDocumentItem(
			CarLoadDocumentItemEntity carLoadDocumentItem,
			TrueMarkWaterIdentificationCode oldTrueMarkWaterCode,
			TrueMarkWaterIdentificationCode newTrueMarkWaterCode)
		{
			var codeToRemove = carLoadDocumentItem.TrueMarkCodes
				.Where(x =>
					x.SourceCode.GTIN == oldTrueMarkWaterCode.GTIN
					&& x.SourceCode.SerialNumber == oldTrueMarkWaterCode.SerialNumber
					&& x.SourceCode.CheckCode == oldTrueMarkWaterCode.CheckCode)
				.First();

			var codeToAdd = new CarLoadDocumentItemTrueMarkProductCode
			{
				SourceCode = newTrueMarkWaterCode,
				ResultCode = newTrueMarkWaterCode,
				Problem = ProductCodeProblem.None,
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				CarLoadDocumentItem = carLoadDocumentItem
			};

			carLoadDocumentItem.TrueMarkCodes.Remove(codeToRemove);
			carLoadDocumentItem.TrueMarkCodes.Add(codeToAdd);

			_uow.Save(carLoadDocumentItem);
			_uow.Commit();
		}

		private void CreateAndSaveCarLoadDocumentLoadingProcessAction(
			int documentId,
			EmployeeWithLogin employee,
			CarLoadDocumentLoadingProcessActionType actionType)
		{
			var action = CreateCarLoadDocumentLoadingProcessAction(documentId, employee.Id, actionType);
			SaveCarLoadDocumentLoadingProcessAction(action);
		}

		private CarLoadDocumentLoadingProcessAction CreateCarLoadDocumentLoadingProcessAction(
			int documentId,
			int employeeId,
			CarLoadDocumentLoadingProcessActionType actionType)
		{
			return new CarLoadDocumentLoadingProcessAction
			{
				CarLoadDocumentId = documentId,
				PickerEmployeeId = employeeId,
				ActionTime = DateTime.Now,
				ActionType = actionType
			};
		}

		private void SaveCarLoadDocumentLoadingProcessAction(CarLoadDocumentLoadingProcessAction action)
		{
			_uow.Save(action);
			_uow.Commit();
		}

		private async Task<bool> CreateStartLoadLogisticEvent(int documentId, string accessToken, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("Выполняем запрос к службе логистических событий для создания события начала сборки." +
					"Параметры запроса:\n" +
					"\tDocumentId: {DocumentId}" +
					"\tAccessToken: {tAccessToken}",
					documentId,
					accessToken);

				var isEventCreated =
					await _logisticsEventsCreationService.CreateStartLoadingWarehouseEvent(documentId, accessToken, cancellationToken);

				_logger.LogInformation("Запрос к службе логистических событий для создания события начала сборки выполнен успешно");

				return isEventCreated;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);
				return false;
			}
		}

		private async Task<bool> CreateEndLoadLogisticEvent(int documentId, string accessToken, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("Выполняем запрос к службе логистических событий для создания события завершения сборки." +
					"Параметры запроса:\n" +
					"\tDocumentId: {DocumentId}" +
					"\tAccessToken: {tAccessToken}",
					documentId,
					accessToken);

				var isEventCreated =
					await _logisticsEventsCreationService.CreateEndLoadingWarehouseEvent(documentId, accessToken, cancellationToken);

				_logger.LogInformation("Запрос к службе логистических событий для создания события завершения сборки выполнен успешно");

				return isEventCreated;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);
				return false;
			}
		}

		private IEnumerable<OrderEdoRequest> CreateEdoRequests(CarLoadDocumentEntity carLoadDocument)
		{
			var carLoadDocumentsItemsNeedsRequest =
				carLoadDocument.Items.Where(x => x.IsIndividualSetForOrder && x.OrderId != null)
				.ToList();

			var edoRequests = new List<OrderEdoRequest>();

			foreach(var item in carLoadDocumentsItemsNeedsRequest)
			{
				var edoRequest = new OrderEdoRequest
				{
					Time = DateTime.Now,
					Source = CustomerEdoRequestSource.Warehouse,
					DocumentType = EdoDocumentType.UPD,
					Order = new OrderEntity { Id = item.OrderId.Value },
				};

				var productCodes = item.TrueMarkCodes
					.Where(x => _trueMarkWaterCodeService.ProductCodesStatusesToCheckDuplicates.Contains(x.SourceCodeStatus));

				foreach(var code in productCodes)
				{
					edoRequest.ProductCodes.Add(code);
				}

				_uow.Save(edoRequest);

				edoRequests.Add(edoRequest);
			}

			_uow.Commit();

			return edoRequests;
		}

		private async Task PublishEdoRequestCreatedEvents(IEnumerable<OrderEdoRequest> edoRequests)
		{
			foreach(var edoRequest in edoRequests)
			{
				await PublishEdoRequestCreatedEvent(edoRequest.Id);
			}
		}

		private async Task PublishEdoRequestCreatedEvent(int requestId)
		{
			_logger.LogInformation(
				"Отправляем событие создания новой заявки на отправку документов ЭДО.  Id заявки: {TaskId}.",
				requestId);

			try
			{
				await _messageBus.Publish(new EdoRequestCreatedEvent { Id = requestId });

				_logger.LogInformation("Событие создания новой заявки на отправку документов ЭДО отправлено успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при отправке события создания новой заявки на отправку документов ЭДО. Id задачи: {TaskId}. Exception: {ExceptionMessage}",
					requestId,
					ex.Message);
			}
		}

		public EmployeeWithLogin GetEmployeeProxyByApiLogin(
			string userLogin,
			ExternalApplicationType applicationType = ExternalApplicationType.WarehouseApp) =>
			_employeeWithLoginRepository.GetEmployeeWithLogin(_uow, userLogin, applicationType);
	}
}
