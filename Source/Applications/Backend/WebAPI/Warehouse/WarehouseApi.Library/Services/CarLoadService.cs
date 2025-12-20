using Edo.Contracts.Messages.Events;
using Gamma.Utilities;
using MassTransit;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using OneOf;
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
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Errors;
using Vodovoz.Models;
using VodovozBusiness.Services.TrueMark;
using WarehouseApi.Contracts.V1.Dto;
using WarehouseApi.Contracts.V1.Responses;
using WarehouseApi.Library.Converters;
using WarehouseApi.Library.Errors;
using WarehouseApi.Library.Extensions;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using CarLoadDocumentErrors = Vodovoz.Errors.Stores.CarLoadDocumentErrors;
using Error = Vodovoz.Core.Domain.Results.Error;

namespace WarehouseApi.Library.Services
{
	public class CarLoadService : ICarLoadService
	{
		private readonly ILogger<CarLoadService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ICarLoadDocumentRepository _carLoadDocumentRepository;
		private readonly IEmployeeWithLoginRepository _employeeWithLoginRepository;
		private readonly IGenericRepository<OrderEntity> _orderRepository;
		private readonly IRouteListDailyNumberProvider _routeListDailyNumberProvider;
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
			IGenericRepository<OrderEntity> orderRepository,
			IRouteListDailyNumberProvider routeListDailyNumberProvider,
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
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListDailyNumberProvider = routeListDailyNumberProvider ?? throw new ArgumentNullException(nameof(routeListDailyNumberProvider));
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

			response.Order.Items
				.PopulateRelatedCodes(_uow, _trueMarkWaterCodeService, documentOrderItems.SelectMany(x=>x.TrueMarkCodes));
			
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
			var pickerEmployee = GetEmployeeProxyByApiLogin(userLogin);

			var trueMarkCodeResult = await _trueMarkWaterCodeService.GetTrueMarkCodeByScannedCode(_uow, scannedCode);

			if(trueMarkCodeResult.IsFailure)
			{
				var error = trueMarkCodeResult.Errors.FirstOrDefault();

				var result = Result.Failure<AddOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, new AddOrderCodeResponse
				{
					Nomenclature = null,
					Result = OperationResultEnumDto.Error,
					Error = error.Message
				});
			}

			try
			{
				trueMarkCodeResult.Value.Match(
					transportCode =>
					{
						_uow.Save(transportCode);
						return true;
					},
					waterGroupCode =>
					{
						_uow.Save(waterGroupCode);
						return true;
					},
					waterIdentificationCode =>
					{
						_uow.Save(waterIdentificationCode);
						return true;
					});
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Exception while commiting: {ExceptionMessage}", e.Message);

				var error = new Error("Database.Commit.Error", e.Message);
				var result = Result.Failure<AddOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, new AddOrderCodeResponse
				{
					Nomenclature = null,
					Result = OperationResultEnumDto.Error,
					Error = error.Message
				});
			}

			_uow.Commit();
			_uow.Session.BeginTransaction();

			if((trueMarkCodeResult.Value.IsTrueMarkTransportCode &&
				trueMarkCodeResult.Value.TrueMarkTransportCode?.ParentTransportCodeId != null)
				|| (trueMarkCodeResult.Value.IsTrueMarkWaterGroupCode
					&& (trueMarkCodeResult.Value.TrueMarkWaterGroupCode?.ParentTransportCodeId != null
					|| trueMarkCodeResult.Value.TrueMarkWaterGroupCode?.ParentWaterGroupCodeId != null))
				|| (trueMarkCodeResult.Value.IsTrueMarkWaterIdentificationCode
					&& (trueMarkCodeResult.Value.TrueMarkWaterIdentificationCode?.ParentTransportCodeId != null
					|| trueMarkCodeResult.Value.TrueMarkWaterIdentificationCode?.ParentWaterGroupCodeId != null)))
			{
				var error = VodovozBusiness.Errors.TrueMark.TrueMarkServiceErrors.AggregationCodeAddError;

				var result = Result.Failure<AddOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, new AddOrderCodeResponse
				{
					Nomenclature = null,
					Result = OperationResultEnumDto.Error,
					Error = error.Message
				});
			}

			IEnumerable<TrueMarkAnyCode> trueMarkAnyCodes = trueMarkCodeResult.Value.Match(
				transportCode => trueMarkAnyCodes = transportCode.GetAllCodes(),
				groupCode => trueMarkAnyCodes = groupCode.GetAllCodes(),
				waterCode => new TrueMarkAnyCode[] { waterCode });

			var allWaterOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var itemsHavingRequiredNomenclature = allWaterOrderItems.Where(item => item.Nomenclature.Id == nomenclatureId).ToList();

			NomenclatureDto nomenclatureDto = null;

			var trueMarkCodes = new List<TrueMarkCodeDto>();

			foreach(var anyCode in trueMarkAnyCodes)
			{
				trueMarkCodes.Add(anyCode.Match(
					PopulateTransportCode(trueMarkAnyCodes),
					PopulateGroupCode(trueMarkAnyCodes),
					PopulateWaterCode(trueMarkAnyCodes)));
			}
			
			var waterCodes =
				trueMarkAnyCodes
				.Where(x => x.IsTrueMarkWaterIdentificationCode)
				.Select(x => x.TrueMarkWaterIdentificationCode)
				.ToArray();

			var addCodesResult =
				await AddCodes(
					orderId,
					nomenclatureId,
					pickerEmployee,
					waterCodes,
					allWaterOrderItems,
					itemsHavingRequiredNomenclature,
					cancellationToken);

			if(addCodesResult.IsT0)
			{
				return addCodesResult.AsT0;
			}

			var documentItemToEdit = addCodesResult.AsT1;

			if(nomenclatureDto is null)
			{
				nomenclatureDto = _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit);
			}

			_uow.Save(documentItemToEdit);

			try
			{
				_uow.Commit();
			}
			catch(MySqlException mysqlException) when (mysqlException.ErrorCode == MySqlErrorCode.DuplicateKey)
			{
				_logger.LogError(mysqlException, "DuplicateEntry: {ExceptionMessage}", mysqlException.Message);

				var error = new Error("Database.Commit.Error", "Код уже был добавлен в другом документе");
				var result = Result.Failure<AddOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, new AddOrderCodeResponse
				{
					Nomenclature = null,
					Result = OperationResultEnumDto.Error,
					Error = error.Message
				});
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Exception while commiting: {ExceptionMessage}", e.Message);

				var error = new Error("Database.Commit.Error", e.Message);
				var result = Result.Failure<AddOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, new AddOrderCodeResponse
				{
					Nomenclature = null,
					Result = OperationResultEnumDto.Error,
					Error = error.Message
				});
			}

			if(nomenclatureDto != null)
			{
				nomenclatureDto.Codes = trueMarkCodes;
			}

			var successResponse = new AddOrderCodeResponse
			{
				Nomenclature = nomenclatureDto,
				Result = OperationResultEnumDto.Success,
				Error = null
			};

			return RequestProcessingResult.CreateSuccess(Result.Success(successResponse));
		}

		private async Task<OneOf<RequestProcessingResult<AddOrderCodeResponse>, CarLoadDocumentItemEntity>> AddCodes(
			int orderId,
			int nomenclatureId,
			EmployeeWithLogin pickerEmployee,
			IEnumerable<TrueMarkWaterIdentificationCode> waterCodes,
			IEnumerable<CarLoadDocumentItemEntity> allWaterOrderItems,
			List<CarLoadDocumentItemEntity> itemsHavingRequiredNomenclature,
			CancellationToken cancellationToken)
		{
			var documentItemToEdit = itemsHavingRequiredNomenclature.FirstOrDefault();

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

			checkResult = await _documentErrorsChecker.IsTrueMarkCodesCanBeAdded(
				orderId,
				nomenclatureId,
				waterCodes,
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

			AddTrueMarkCodesToCarLoadDocumentItem(documentItemToEdit, waterCodes);

			return await Task.FromResult(documentItemToEdit);
		}

		public async Task<RequestProcessingResult<ChangeOrderCodeResponse>> ChangeOrderCode(
			int orderId,
			int nomenclatureId,
			string oldScannedCode,
			string newScannedCode,
			string userLogin,
			CancellationToken cancellationToken)
		{
			var pickerEmployee = GetEmployeeProxyByApiLogin(userLogin);

			var oldTrueMarkCodeResult = await _trueMarkWaterCodeService.GetTrueMarkCodeByScannedCode(_uow, oldScannedCode);

			if(oldTrueMarkCodeResult.IsFailure)
			{
				var error = oldTrueMarkCodeResult.Errors.FirstOrDefault();
				var result = Result.Failure<ChangeOrderCodeResponse>(error);
				return RequestProcessingResult.CreateFailure(result, new ChangeOrderCodeResponse
				{
					Nomenclature = null,
					Result = OperationResultEnumDto.Error,
					Error = error.Message
				});
			}

			Vodovoz.Core.Domain.Results.Result<TrueMarkAnyCode> newTrueMarkCodeResult = null;

			if(!string.IsNullOrWhiteSpace(newScannedCode))
			{
				newTrueMarkCodeResult = await _trueMarkWaterCodeService.GetTrueMarkCodeByScannedCode(_uow, newScannedCode);

				if(newTrueMarkCodeResult.IsFailure)
				{
					var error = newTrueMarkCodeResult.Errors.FirstOrDefault();
					var result = Result.Failure<ChangeOrderCodeResponse>(error);
					return RequestProcessingResult.CreateFailure(result, new ChangeOrderCodeResponse
					{
						Nomenclature = null,
						Result = OperationResultEnumDto.Error,
						Error = error.Message
					});
				}
				try
				{
					newTrueMarkCodeResult.Value.Match(
						transportCode =>
						{
							_uow.Save(transportCode);
							return true;
						},
						waterGroupCode =>
						{
							_uow.Save(waterGroupCode);
							return true;
						},
						waterIdentificationCode =>
						{
							_uow.Save(waterIdentificationCode);
							return true;
						});

					_uow.Commit();
					_uow.Session.BeginTransaction();
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Exception while commiting: {ExceptionMessage}", e.Message);

					var error = new Error("Database.Commit.Error", e.Message);
					var result = Result.Failure<ChangeOrderCodeResponse>(error);
					return RequestProcessingResult.CreateFailure(result, new ChangeOrderCodeResponse
					{
						Nomenclature = null,
						Result = OperationResultEnumDto.Error,
						Error = error.Message
					});
				}
			}

			if(oldTrueMarkCodeResult.Value.Match(
				transportCode => transportCode.ParentTransportCodeId != null,
				groupCode => groupCode.ParentTransportCodeId != null || groupCode.ParentWaterGroupCodeId != null,
				waterCode => waterCode.ParentTransportCodeId != null || waterCode.ParentWaterGroupCodeId != null))
			{
				var error = VodovozBusiness.Errors.TrueMark
					.TrueMarkServiceErrors.AggregationCodeChangeError;
				var result = Result.Failure<ChangeOrderCodeResponse>(error);
				return RequestProcessingResult.CreateFailure(result, new ChangeOrderCodeResponse
				{
					Nomenclature = null,
					Result = OperationResultEnumDto.Error,
					Error = error.Message
				});
			}

			if(newTrueMarkCodeResult != null
				&& newTrueMarkCodeResult.Value.Match(
				transportCode => transportCode.ParentTransportCodeId != null,
				groupCode => groupCode.ParentTransportCodeId != null || groupCode.ParentWaterGroupCodeId != null,
				waterCode => waterCode.ParentTransportCodeId != null || waterCode.ParentWaterGroupCodeId != null))
			{
				var error = VodovozBusiness.Errors.TrueMark.TrueMarkServiceErrors.AggregationCodeAddError;
				var result = Result.Failure<ChangeOrderCodeResponse>(error);
				return RequestProcessingResult.CreateFailure(result, new ChangeOrderCodeResponse
				{
					Nomenclature = null,
					Result = OperationResultEnumDto.Error,
					Error = error.Message
				});
			}

			var allWaterOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var itemsHavingRequiredNomenclature = allWaterOrderItems.Where(item => item.Nomenclature.Id == nomenclatureId).ToList();

			IEnumerable<TrueMarkAnyCode> oldTrueMarkAnyCodes = oldTrueMarkCodeResult.Value.Match(
				transportCode => transportCode.GetAllCodes(),
				groupCode => groupCode.GetAllCodes(),
				waterCode => new TrueMarkAnyCode[] { waterCode });

			IEnumerable<TrueMarkAnyCode> newTrueMarkAnyCodes = newTrueMarkCodeResult?.Value.Match(
				transportCode => transportCode.GetAllCodes(),
				groupCode => groupCode.GetAllCodes(),
				waterCode => new TrueMarkAnyCode[] { waterCode }) ?? Enumerable.Empty<TrueMarkAnyCode>();

			var oldTrueMarkAnyCodesList = oldTrueMarkAnyCodes.ToArray();

			foreach(var codeToRemove in oldTrueMarkAnyCodesList)
			{
				if(!codeToRemove.IsTrueMarkWaterIdentificationCode)
				{
					continue;
				}

				await RemoveSingleCode(_uow, userLogin, codeToRemove.TrueMarkWaterIdentificationCode, allWaterOrderItems, itemsHavingRequiredNomenclature, cancellationToken);
			}

			foreach(var oldCodeToRemoveFromDatabase in oldTrueMarkAnyCodesList)
			{
				oldCodeToRemoveFromDatabase.Match(
					transportCode =>
					{
						transportCode.ClearAllCodes();
						return true;
					},
					groupCode =>
					{
						groupCode.ClearAllCodes();
						return true;
					},
					waterCode =>
					{
						return true;
					});
			}

			try
			{
				_uow.Commit();
				_uow.Session.BeginTransaction();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Exception while commiting: {ExceptionMessage}", e.Message);
			}

			foreach(var oldCodeToRemoveFromDatabase in oldTrueMarkAnyCodesList)
			{
				oldCodeToRemoveFromDatabase.Match(
					transportCode =>
					{
						_uow.Delete(transportCode);
						return true;
					},
					groupCode =>
					{
						_uow.Delete(groupCode);
						return true;
					},
					waterCode =>
					{
						_uow.Delete(waterCode);
						return true;
					});
			}

			try
			{
				_uow.Commit();
				_uow.Session.BeginTransaction();
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Exception while commiting: {ExceptionMessage}", e.Message);
			}

			NomenclatureDto nomenclatureDto = null;

			var trueMarkCodes = new List<TrueMarkCodeDto>();

			foreach(var anyCode in newTrueMarkAnyCodes)
			{
				trueMarkCodes.Add(anyCode.Match(
					PopulateTransportCode(newTrueMarkAnyCodes),
					PopulateGroupCode(newTrueMarkAnyCodes),
					PopulateWaterCode(newTrueMarkAnyCodes)));
			}

			CarLoadDocumentItemEntity documentItemToEdit = null;

			var codesToAdd = 
				newTrueMarkAnyCodes
				.Where(x => x.IsTrueMarkWaterIdentificationCode)
				.Select(x => x.TrueMarkWaterIdentificationCode)
				.ToArray();

			var addCodesResult =
				await AddCodes(
					orderId,
					nomenclatureId,
					pickerEmployee,
					codesToAdd,
					allWaterOrderItems,
					itemsHavingRequiredNomenclature,
					cancellationToken);

			if(addCodesResult.IsT0)
			{
				var otherFailureResult = addCodesResult.AsT0;
				return RequestProcessingResult.CreateFailure(
					Result.Failure<ChangeOrderCodeResponse>(otherFailureResult.Result.Errors),
					new ChangeOrderCodeResponse
					{
						Nomenclature = otherFailureResult.FailureData.Nomenclature,
						Result = otherFailureResult.FailureData.Result,
						Error = otherFailureResult.FailureData.Error,
					});
			}

			documentItemToEdit = addCodesResult.AsT1;

			if(nomenclatureDto is null)
			{
				nomenclatureDto = _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit);
			}

			_uow.Save(documentItemToEdit);
			_uow.Commit();

			if(nomenclatureDto != null)
			{
				nomenclatureDto.Codes = trueMarkCodes;
			}

			var successResponse = new ChangeOrderCodeResponse
			{
				Nomenclature = nomenclatureDto,
				Result = OperationResultEnumDto.Success,
				Error = null
			};

			return RequestProcessingResult.CreateSuccess(Result.Success(successResponse));
		}

		private async Task<OneOf<RequestProcessingResult<ChangeOrderCodeResponse>, CarLoadDocumentItemEntity>> RemoveSingleCode(
			IUnitOfWork unitOfWork,
			string userLogin,
			TrueMarkWaterIdentificationCode oldTrueMarkWaterCode,
			IEnumerable<CarLoadDocumentItemEntity> allWaterOrderItems,
			List<CarLoadDocumentItemEntity> itemsHavingRequiredNomenclature,
			CancellationToken cancellationToken)
		{
			CarLoadDocumentItemEntity documentItemToEdit = itemsHavingRequiredNomenclature.FirstOrDefault();
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

			var resultOfRemoving = RemoveTrueMarkCodeInCarLoadDocumentItem(unitOfWork, documentItemToEdit, oldTrueMarkWaterCode);

			if(resultOfRemoving.IsFailure)
			{
				return RequestProcessingResult.CreateFailure(
					Result.Failure<ChangeOrderCodeResponse>(resultOfRemoving.Errors),
					failureResponse);
			}

			return await Task.FromResult(documentItemToEdit);
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

			var edoRequests = CreateEdoRequests(carLoadDocument);

			_uow.Commit();

			await PublishEdoRequestCreatedEvents(edoRequests);

			var successResponse = new EndLoadResponse
			{
				Result = OperationResultEnumDto.Success,
				Error = null
			};

			return RequestProcessingResult.CreateSuccess(Result.Success(successResponse));
		}

		private async Task<IEnumerable<CarLoadDocumentItemEntity>> GetCarLoadDocumentWaterOrderItems(int orderId)
		{
			_logger.LogInformation("Получаем данные по заказу #{OrderId} из талона погрузки", orderId);
			var documentOrderItems =
				await _carLoadDocumentRepository.GetAccountableInTrueMarkHavingGtinItemsByCarLoadDocumentId(_uow, orderId);

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

		private void AddTrueMarkCodesToCarLoadDocumentItem(
			CarLoadDocumentItemEntity carLoadDocumentItem,
			IEnumerable<TrueMarkWaterIdentificationCode> trueMarkWaterCode)
		{
			foreach(var code in trueMarkWaterCode)
			{
				AddTrueMarkCodeToCarLoadDocumentItem(carLoadDocumentItem, code);
			}
		}

		private void AddTrueMarkCodeToCarLoadDocumentItem(CarLoadDocumentItemEntity carLoadDocumentItem, TrueMarkWaterIdentificationCode trueMarkWaterCode)
		{
			if(trueMarkWaterCode.Id == 0)
			{
				_uow.Save(trueMarkWaterCode);
			}

			var productCode = new CarLoadDocumentItemTrueMarkProductCode
			{
				CreationTime = DateTime.Now,
				SourceCode = trueMarkWaterCode,
				ResultCode = trueMarkWaterCode,
				Problem = ProductCodeProblem.None,
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				CarLoadDocumentItem = carLoadDocumentItem
			};

			_uow.Save(productCode);

			carLoadDocumentItem.TrueMarkCodes.Add(productCode);
		}

		private Result RemoveTrueMarkCodeInCarLoadDocumentItem(
			IUnitOfWork uow,
			CarLoadDocumentItemEntity carLoadDocumentItem,
			TrueMarkWaterIdentificationCode oldTrueMarkWaterCode)
		{
			var codeToRemove = carLoadDocumentItem.TrueMarkCodes
				.Where(x =>
					x.SourceCode.Gtin == oldTrueMarkWaterCode.Gtin
					&& x.SourceCode.SerialNumber == oldTrueMarkWaterCode.SerialNumber
					&& x.SourceCode.CheckCode == oldTrueMarkWaterCode.CheckCode)
				.FirstOrDefault();

			if(codeToRemove is null)
			{
				return Result.Failure(VodovozBusiness.Errors.TrueMark.TrueMarkServiceErrors.MissingTrueMarkCodeToDelete);
			}

			carLoadDocumentItem.TrueMarkCodes.Remove(codeToRemove);

			uow.Delete(codeToRemove);

			return Result.Success();
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

		private IEnumerable<PrimaryEdoRequest> CreateEdoRequests(CarLoadDocumentEntity carLoadDocument)
		{
			var ordersNeedsRequest =
				carLoadDocument.Items
				.Where(x => x.IsIndividualSetForOrder && x.OrderId != null)
				.GroupBy(x => x.OrderId)
				.ToDictionary(x => x.Key, x => x.SelectMany(c => c.TrueMarkCodes).ToList());

			var orders = _orderRepository.Get(_uow, x => ordersNeedsRequest.Keys.Contains(x.Id));

			var edoRequests = new List<PrimaryEdoRequest>();

			foreach(var item in ordersNeedsRequest)
			{
				var orderId = item.Key;
				var trueMarkCodes = item.Value;

				var order = orders.Where(x => x.Id == orderId).FirstOrDefault();

				if(order?.IsClientWorksWithNewEdoProcessing == false)
				{
					continue;
				}

				var edoRequest = new PrimaryEdoRequest
				{
					Time = DateTime.Now,
					Source = CustomerEdoRequestSource.Warehouse,
					DocumentType = EdoDocumentType.UPD,
					Order = orders.Where(x => x.Id == orderId).FirstOrDefault(),
				};

				var productCodes = trueMarkCodes
					.Where(x => _trueMarkWaterCodeService.SuccessfullyUsedProductCodesStatuses.Contains(x.SourceCodeStatus));

				foreach(var code in productCodes)
				{
					edoRequest.ProductCodes.Add(code);
				}

				_uow.Save(edoRequest);

				edoRequests.Add(edoRequest);
			}

			return edoRequests;
		}

		private async Task PublishEdoRequestCreatedEvents(IEnumerable<PrimaryEdoRequest> edoRequests)
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

		private static Func<TrueMarkWaterIdentificationCode, TrueMarkCodeDto> PopulateWaterCode(IEnumerable<TrueMarkAnyCode> allCodes)
		{
			return waterCode =>
			{
				string parentRawCode = null;

				if(waterCode.ParentTransportCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkTransportCode
							&& x.TrueMarkTransportCode.Id == waterCode.ParentTransportCodeId)
						?.TrueMarkTransportCode.RawCode;
				}

				if(waterCode.ParentWaterGroupCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkWaterGroupCode
							&& x.TrueMarkWaterGroupCode.Id == waterCode.ParentWaterGroupCodeId)
						?.TrueMarkWaterGroupCode.RawCode;
				}

				return new TrueMarkCodeDto
				{
					Code = waterCode.RawCode,
					Level = WarehouseApiTruemarkCodeLevel.unit,
					Parent = parentRawCode,
				};
			};
		}

		private static Func<TrueMarkWaterGroupCode, TrueMarkCodeDto> PopulateGroupCode(IEnumerable<TrueMarkAnyCode> allCodes)
		{
			return groupCode =>
			{
				string parentRawCode = null;

				if(groupCode.ParentTransportCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkTransportCode
							&& x.TrueMarkTransportCode.Id == groupCode.ParentTransportCodeId)
						?.TrueMarkTransportCode.RawCode;
				}

				if(groupCode.ParentWaterGroupCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkWaterGroupCode
							&& x.TrueMarkWaterGroupCode.Id == groupCode.ParentWaterGroupCodeId)
						?.TrueMarkWaterGroupCode.RawCode;
				}

				return new TrueMarkCodeDto
				{
					Code = groupCode.RawCode,
					Level = WarehouseApiTruemarkCodeLevel.group,
					Parent = parentRawCode
				};
			};
		}

		private static Func<TrueMarkTransportCode, TrueMarkCodeDto> PopulateTransportCode(IEnumerable<TrueMarkAnyCode> allCodes)
		{
			return transportCode =>
			{
				string parentRawCode = null;

				if(transportCode.ParentTransportCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkTransportCode
							&& x.TrueMarkTransportCode.Id == transportCode.ParentTransportCodeId)
						?.TrueMarkTransportCode.RawCode;
				}

				return new TrueMarkCodeDto
				{
					Code = transportCode.RawCode,
					Level = WarehouseApiTruemarkCodeLevel.transport,
					Parent = parentRawCode
				};
			};
		}
	}
}
