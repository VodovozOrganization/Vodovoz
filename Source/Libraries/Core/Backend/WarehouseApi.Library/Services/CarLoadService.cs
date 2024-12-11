using Edo.Transport.Messages.Events;
using Gamma.Utilities;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Data.Interfaces.Employees;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Errors;
using Vodovoz.Models;
using Vodovoz.Models.TrueMark;
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
		private readonly CarLoadDocumentConverter _carLoadDocumentConverter;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
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
			CarLoadDocumentConverter carLoadDocumentConverter,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
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
			_carLoadDocumentConverter = carLoadDocumentConverter ?? throw new ArgumentNullException(nameof(carLoadDocumentConverter));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
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

			if(!_documentErrorsChecker.IsEmployeeCanPickUpCarLoadDocument(documentId, pickerEmployee, out Error error))
			{
				failureResponse.Error = error.Message;

				var result = Result.Failure<StartLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			CreateAndSaveCarLoadDocumentLoadingProcessAction(carLoadDocument?.Id ?? 0, pickerEmployee, CarLoadDocumentLoadingProcessActionType.StartLoad);

			if(!_documentErrorsChecker.IsCarLoadDocumentLoadingCanBeStarted(carLoadDocument, documentId, out error))
			{
				failureResponse.Error = error.Message;

				var result = Result.Failure<StartLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			var isDocumentSavedAndEventsCreated =
				await SetLoadOperationStateAndSaveDocument(carLoadDocument, CarLoadDocumentLoadOperationState.InProgress, accessToken, cancellationToken);

			if(!isDocumentSavedAndEventsCreated)
			{
				error = CarLoadDocumentErrors.CreateCarLoadDocumentStateChangeError(carLoadDocument.Id);
				failureResponse.Error = error.Message;

				var result = Result.Failure<StartLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

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

			if(!_documentErrorsChecker.IsOrderNeedIndividualSetOnLoad(orderId, out Error error)
				|| !_documentErrorsChecker.IsItemsHavingRequiredOrderExistsAndIncludedInOnlyOneDocument(orderId, documentOrderItems, out error))
			{
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
			var isScannedCodeValid = _trueMarkWaterCodeParser.TryParse(scannedCode, out TrueMarkWaterCode trueMarkCode);

			var allWaterOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var itemsHavingRequiredNomenclature = allWaterOrderItems.Where(item => item.Nomenclature.Id == nomenclatureId).ToList();
			var documentItemToEdit = itemsHavingRequiredNomenclature.FirstOrDefault();

			var pickerEmployee = GetEmployeeProxyByApiLogin(userLogin);

			var failureResponse = new AddOrderCodeResponse
			{
				Nomenclature = documentItemToEdit is null ? null : _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit),
				Result = OperationResultEnumDto.Error,
			};

			if(!_documentErrorsChecker.IsEmployeeCanPickUpCarLoadDocument(documentItemToEdit?.Document?.Id ?? 0, pickerEmployee, out Error error))
			{
				failureResponse.Error = error.Message;

				var result = Result.Failure<AddOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			CreateAndSaveCarLoadDocumentLoadingProcessAction(
				documentItemToEdit?.Document?.Id ?? 0,
				pickerEmployee,
				CarLoadDocumentLoadingProcessActionType.AddTrueMarkCode);

			if(!_documentErrorsChecker.IsTrueMarkCodeCanBeAdded(
				orderId,
				nomenclatureId,
				scannedCode,
				isScannedCodeValid,
				trueMarkCode,
				allWaterOrderItems,
				itemsHavingRequiredNomenclature,
				documentItemToEdit,
				cancellationToken,
				out error))
			{
				failureResponse.Error = error.Message;

				var result = Result.Failure<AddOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			AddTrueMarkCodeAndSaveCarLoadDocumentItem(documentItemToEdit, trueMarkCode);

			var successResponse = new AddOrderCodeResponse
			{
				Nomenclature = documentItemToEdit is null ? null : _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit),
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
			var isOldScannedCodeValid = _trueMarkWaterCodeParser.TryParse(oldScannedCode, out TrueMarkWaterCode oldTrueMarkCode);
			var isNewScannedCodeValid = _trueMarkWaterCodeParser.TryParse(newScannedCode, out TrueMarkWaterCode newTrueMarkCode);

			var allWaterOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var itemsHavingRequiredNomenclature = allWaterOrderItems.Where(item => item.Nomenclature.Id == nomenclatureId).ToList();
			var documentItemToEdit = itemsHavingRequiredNomenclature.FirstOrDefault();

			var pickerEmployee = GetEmployeeProxyByApiLogin(userLogin);

			var failureResponse = new ChangeOrderCodeResponse
			{
				Nomenclature = documentItemToEdit is null ? null : _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit),
				Result = OperationResultEnumDto.Error,
			};

			if(!_documentErrorsChecker.IsEmployeeCanPickUpCarLoadDocument(documentItemToEdit?.Document?.Id ?? 0, pickerEmployee, out Error error))
			{
				failureResponse.Error = error.Message;

				var result = Result.Failure<ChangeOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			CreateAndSaveCarLoadDocumentLoadingProcessAction(
				documentItemToEdit?.Document?.Id ?? 0,
				pickerEmployee,
				CarLoadDocumentLoadingProcessActionType.ChangeTrueMarkCode);

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
				cancellationToken,
				out error))
			{
				failureResponse.Error = error.Message;

				var result = Result.Failure<ChangeOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			ChangeTrueMarkCodeAndSaveCarLoadDocumentItem(documentItemToEdit, oldTrueMarkCode, newTrueMarkCode);

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

			if(!_documentErrorsChecker.IsEmployeeCanPickUpCarLoadDocument(documentId, pickerEmployee, out Error error))
			{
				failureResponse.Error = error.Message;

				var result = Result.Failure<EndLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			CreateAndSaveCarLoadDocumentLoadingProcessAction(carLoadDocument?.Id ?? 0, pickerEmployee, CarLoadDocumentLoadingProcessActionType.EndLoad);

			if(!_documentErrorsChecker.IsCarLoadDocumentLoadingCanBeDone(carLoadDocument, documentId, out error))
			{
				failureResponse.Error = error.Message;

				var result = Result.Failure<EndLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			var isDocumentSavedAndEventsCreated =
				await SetLoadOperationStateAndSaveDocument(carLoadDocument, CarLoadDocumentLoadOperationState.Done, accessToken, cancellationToken);

			if(!isDocumentSavedAndEventsCreated)
			{
				error = CarLoadDocumentErrors.CreateCarLoadDocumentStateChangeError(carLoadDocument.Id);

				failureResponse.Error = error.Message;

				var result = Result.Failure<EndLoadResponse>(error);

				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			var successResponse = new EndLoadResponse
			{
				Result = OperationResultEnumDto.Success,
				Error = null
			};

			await PublishRequestCreatedEvent(11);

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

		private async Task<bool> SetLoadOperationStateAndSaveDocument(
			CarLoadDocumentEntity document,
			CarLoadDocumentLoadOperationState newLoadOperationState,
			string logisticsEventApiAccessToken,
			CancellationToken cancellationToken)
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

			var isLogisticsEventCreated =
				newLoadOperationState == CarLoadDocumentLoadOperationState.InProgress
				? await CreateStartLoadLogisticEvent(document.Id, logisticsEventApiAccessToken, cancellationToken)
				: await CreateEndLoadLogisticEvent(document.Id, logisticsEventApiAccessToken, cancellationToken);

			if(!isLogisticsEventCreated)
			{
				_logger.LogInformation("Логистическое событие для талона погрузки не было создано. Отменяем изменение статуса талона погрузки.");
				_uow.Session.GetCurrentTransaction().Rollback();
				return false;
			}

			_uow.Commit();

			_logger.LogInformation("Статус талона погрузки #{DocumentId} успешно изменен на \"{NewStatus}\"",
				document.Id,
				newLoadOperationState.GetEnumTitle());

			return true;
		}

		private void AddTrueMarkCodeAndSaveCarLoadDocumentItem(CarLoadDocumentItemEntity carLoadDocumentItem, TrueMarkWaterCode trueMarkCode)
		{
			var codeEntity = CreateTrueMarkCodeEntity(trueMarkCode);

			carLoadDocumentItem.TrueMarkCodes.Add(new CarLoadDocumentItemTrueMarkProductCode
			{
				SourceCode = codeEntity,
				ResultCode = codeEntity,
				Problem = ProductCodeProblem.None,
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				CarLoadDocumentItem = carLoadDocumentItem
			});

			_uow.Save(carLoadDocumentItem);
			_uow.Commit();
		}

		private void ChangeTrueMarkCodeAndSaveCarLoadDocumentItem(CarLoadDocumentItemEntity carLoadDocumentItem, TrueMarkWaterCode oldTrueMarkCode, TrueMarkWaterCode newTrueMarkCode)
		{
			var codeToRemove = carLoadDocumentItem.TrueMarkCodes
				.Where(x =>
					x.SourceCode.GTIN == oldTrueMarkCode.GTIN
					&& x.SourceCode.SerialNumber == oldTrueMarkCode.SerialNumber
					&& x.SourceCode.CheckCode == oldTrueMarkCode.CheckCode)
				.First();

			var codeEntity = CreateTrueMarkCodeEntity(newTrueMarkCode);

			var codeToAdd = new CarLoadDocumentItemTrueMarkProductCode
			{
				SourceCode = codeEntity,
				ResultCode = codeEntity,
				Problem = ProductCodeProblem.None,
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				CarLoadDocumentItem = carLoadDocumentItem
			};

			carLoadDocumentItem.TrueMarkCodes.Remove(codeToRemove);
			carLoadDocumentItem.TrueMarkCodes.Add(codeToAdd);

			_uow.Save(carLoadDocumentItem);
			_uow.Commit();
		}

		private TrueMarkWaterIdentificationCode CreateTrueMarkCodeEntity(TrueMarkWaterCode trueMarkCode)
		{
			var codeEntity = new TrueMarkWaterIdentificationCode
			{
				IsInvalid = false,
				RawCode = trueMarkCode.SourceCode?.Substring(0, Math.Min(255, trueMarkCode.SourceCode.Length)),
				GTIN = trueMarkCode.GTIN,
				SerialNumber = trueMarkCode.SerialNumber,
				CheckCode = trueMarkCode.CheckCode
			};

			return codeEntity;
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

		private async Task PublishRequestCreatedEvent(int taskId)
		{
			_logger.LogInformation(
				"Отправляем событие создания новой заявки на отправку документов ЭДО.  Id заявки: {TaskId}.",
				taskId);

			try
			{
				await _messageBus.Publish(new EdoRequestCreatedEvent { Id = taskId });

				_logger.LogInformation("Событие создания новой заявки на отправку документов ЭДО отправлено успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при отправке события создания новой заявки на отправку документов ЭДО. Id задачи: {TaskId}. Exception: {ExceptionMessage}",
					taskId,
					ex.Message);
			}
		}

		public EmployeeWithLogin GetEmployeeProxyByApiLogin(
			string userLogin,
			ExternalApplicationType applicationType = ExternalApplicationType.WarehouseApp) =>
			_employeeWithLoginRepository.GetEmployeeWithLogin(_uow, userLogin, applicationType);
	}
}
