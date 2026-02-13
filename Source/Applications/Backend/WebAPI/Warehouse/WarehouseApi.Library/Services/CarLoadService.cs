using Edo.Contracts.Messages.Events;
using Gamma.Utilities;
using MassTransit;
using Microsoft.Extensions.Logging;
using MoreLinq;
using MySqlConnector;
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
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Errors;
using Vodovoz.Errors.Orders;
using Vodovoz.Models;
using VodovozBusiness.Domain.Client.Specifications;
using VodovozBusiness.Services.TrueMark;
using WarehouseApi.Contracts.V1.Dto;
using WarehouseApi.Contracts.V1.Responses;
using WarehouseApi.Library.Converters;
using WarehouseApi.Library.Errors;
using WarehouseApi.Library.Extensions;
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
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly IGenericRepository<SelfDeliveryDocument> _selfDeliveryRepository;
		private readonly IRouteListDailyNumberProvider _routeListDailyNumberProvider;
		private readonly ILogisticsEventsCreationService _logisticsEventsCreationService;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly CarLoadDocumentConverter _carLoadDocumentConverter;
		private readonly CarLoadDocumentProcessingErrorsChecker _documentErrorsChecker;
		private readonly ICarLoadDocumentTrueMarkCodesProcessingService _codesProcessingService;
		private readonly IGenericRepository<StagingTrueMarkCode> _stagingTrueMarkCodeRepository;
		private readonly IBus _messageBus;

		public CarLoadService(
			ILogger<CarLoadService> logger,
			IUnitOfWork uow,
			ICarLoadDocumentRepository carLoadDocumentRepository,
			IEmployeeWithLoginRepository employeeWithLoginRepository,
			IGenericRepository<Order> orderRepository,
			IGenericRepository<SelfDeliveryDocument> selfDeliveryRepository,
			IRouteListDailyNumberProvider routeListDailyNumberProvider,
			ILogisticsEventsCreationService logisticsEventsCreationService,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			CarLoadDocumentConverter carLoadDocumentConverter,
			CarLoadDocumentProcessingErrorsChecker documentErrorsChecker,
			ICarLoadDocumentTrueMarkCodesProcessingService codesProcessingService,
			IGenericRepository<StagingTrueMarkCode> stagingTrueMarkCodeRepository,
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_carLoadDocumentRepository = carLoadDocumentRepository ?? throw new ArgumentNullException(nameof(carLoadDocumentRepository));
			_employeeWithLoginRepository = employeeWithLoginRepository;
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_selfDeliveryRepository = selfDeliveryRepository ?? throw new ArgumentNullException(nameof(selfDeliveryRepository));
			_routeListDailyNumberProvider = routeListDailyNumberProvider ?? throw new ArgumentNullException(nameof(routeListDailyNumberProvider));
			_logisticsEventsCreationService = logisticsEventsCreationService ?? throw new ArgumentNullException(nameof(logisticsEventsCreationService));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_carLoadDocumentConverter = carLoadDocumentConverter ?? throw new ArgumentNullException(nameof(carLoadDocumentConverter));
			_documentErrorsChecker = documentErrorsChecker ?? throw new ArgumentNullException(nameof(documentErrorsChecker));
			_codesProcessingService = codesProcessingService ?? throw new ArgumentNullException(nameof(codesProcessingService));
			_stagingTrueMarkCodeRepository = stagingTrueMarkCodeRepository ?? throw new ArgumentNullException(nameof(stagingTrueMarkCodeRepository));
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

		public async Task<RequestProcessingResult<GetOrderResponse>> GetOrder(int orderId, CancellationToken cancellationToken)
		{
			var order =
				(await _orderRepository.GetAsync(_uow, x => x.Id == orderId, cancellationToken: cancellationToken))
				.Value
				.FirstOrDefault();

			if(order is null)
			{
				var error = OrderErrors.NotFound;
				var result = Result.Failure<GetOrderResponse>(error);
				return RequestProcessingResult.CreateFailure(
					result, 
					new GetOrderResponse
					{
						Result = OperationResultEnumDto.Error,
						Error = error.Message
					});
			}

			if(order.SelfDelivery)
			{
				return await GetSelfDeliveryDocumentOrder(order, cancellationToken);
			}

			return await GetCarLoadDocumentOrder(orderId, cancellationToken);
		}

		private async Task<RequestProcessingResult<GetOrderResponse>> GetSelfDeliveryDocumentOrder(Order order, CancellationToken cancellationToken)
		{
			var selfDeliveryDocument =
				(await _selfDeliveryRepository.GetAsync(_uow, x => x.Order.Id == order.Id, cancellationToken: cancellationToken))
				.Value
				.FirstOrDefault();

			var nomenclatures = order.OrderItems
				.Select(x => x.Nomenclature)
				.ToArray();

			var orderDto = order.ToApiDtoV1(nomenclatures, selfDeliveryDocument);

			if(selfDeliveryDocument != null)
			{
				orderDto.Items
					.PopulateRelatedCodes(_uow, _trueMarkWaterCodeService, selfDeliveryDocument.Items.SelectMany(x => x.TrueMarkProductCodes));
			}

			orderDto.Items.ForEach(item =>
				item.Codes.ForEach((code, i) =>
					code.SequenceNumber = i));

			var response = new GetOrderResponse
			{
				Order = orderDto
			};

			return RequestProcessingResult.CreateSuccess(Result.Success(response));
		}

		private async Task<RequestProcessingResult<GetOrderResponse>> GetCarLoadDocumentOrder(int orderId, CancellationToken cancellationToken)
		{
			var documentOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var carLoadDocument = documentOrderItems.FirstOrDefault().Document;

			var response = new GetOrderResponse();

			if(carLoadDocument is null)
			{
				var error = CarLoadDocumentErrors.CreateCarLoadDocumentItemNotFound(orderId);
				var result = Result.Failure<GetOrderResponse>(error);
				return RequestProcessingResult.CreateFailure(result, response);
			}
			
			var carLoadDocumentItemsStagingCodes = await GetCarLoadDocumentOrderItemsStagingCodes(
				documentOrderItems.Select(x => x.Id),
				cancellationToken);

			response.Order = _carLoadDocumentConverter.ConvertToApiOrder(documentOrderItems, carLoadDocumentItemsStagingCodes);

			if(carLoadDocument.LoadOperationState == CarLoadDocumentLoadOperationState.Done)
			{
				foreach(var documentOrderItem in documentOrderItems)
				{
					foreach(var trueMarkProductCode in documentOrderItem.TrueMarkCodes)
					{
						if(trueMarkProductCode.ResultCode == null)
						{
							continue;
						}

						if(trueMarkProductCode.ResultCode.ParentWaterGroupCodeId == null
							&& trueMarkProductCode.ResultCode.ParentTransportCodeId == null)
						{
							continue;
						}

						var codeToAddInfo = response.Order.Items.FirstOrDefault(x => x.Codes.Select(code => code.Code).Contains(trueMarkProductCode.ResultCode.RawCode));

						if(codeToAddInfo.Codes.Any(x => x.Parent != null && x.Code == trueMarkProductCode.ResultCode.RawCode))
						{
							continue;
						}

						var parentCode = _trueMarkWaterCodeService.GetParentGroupCode(_uow, trueMarkProductCode.ResultCode);

						if(codeToAddInfo is null)
						{
							continue;
						}

						var trueMarkCodes = new List<TrueMarkCodeDto>();

						var allCodes = parentCode.Match(
							transportCode => transportCode.GetAllCodes(),
							groupCode => groupCode.GetAllCodes(),
							waterCode => new TrueMarkAnyCode[] { waterCode })
							.ToArray();

						var codesInCurrentOrder = allCodes.Where(x => x.IsTrueMarkWaterIdentificationCode
							&& documentOrderItem.TrueMarkCodes.Any(y =>
								(y.ResultCode != null && y.ResultCode.Id == x.TrueMarkWaterIdentificationCode.Id)
								|| (y.SourceCode != null && y.SourceCode.Id == x.TrueMarkWaterIdentificationCode.Id)))
							.Select(x => x.TrueMarkWaterIdentificationCode)
							.ToArray();

						foreach(var anyCode in allCodes)
						{
							if(anyCode.IsTrueMarkWaterIdentificationCode
								&& !codesInCurrentOrder.Any(x => x.Id == anyCode.TrueMarkWaterIdentificationCode.Id))
							{
								continue;
							}

							trueMarkCodes.Add(
								anyCode.Match(
									PopulateTransportCode(allCodes),
									PopulateGroupCode(allCodes),
									PopulateWaterCode(allCodes)));
						}

						codeToAddInfo.Codes.RemoveAll(code => trueMarkCodes.Any(x => x.Code == code.Code));
						codeToAddInfo.Codes.AddRange(trueMarkCodes);
					}
				}
			}

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

			var allWaterOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var itemsHavingRequiredNomenclature = allWaterOrderItems.Where(item => item.Nomenclature.Id == nomenclatureId).ToList();
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

			var isCodesCanBeAdded = _documentErrorsChecker.IsTrueMarkCodesCanBeAdded(
				orderId,
				nomenclatureId,
				allWaterOrderItems,
				itemsHavingRequiredNomenclature,
				documentItemToEdit);

			if(isCodesCanBeAdded.IsFailure)
			{
				var error = isCodesCanBeAdded.Errors.FirstOrDefault();
				failureResponse.Error = error.Message;
				var result = Result.Failure<AddOrderCodeResponse>(error);
				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			var addCodesResult =
				await _codesProcessingService.AddStagingTrueMarkCode(
					_uow,
					scannedCode,
					documentItemToEdit,
					cancellationToken);

			if(addCodesResult.IsFailure)
			{
				var error = addCodesResult.Errors.FirstOrDefault();
				failureResponse.Error = error.Message;
				var result = Result.Failure<AddOrderCodeResponse>(error);
				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			try
			{
				_uow.Commit();
			}
			catch(MySqlException mysqlException) when(mysqlException.ErrorCode == MySqlErrorCode.DuplicateKey)
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

			NomenclatureDto nomenclatureDto = null;

			if(nomenclatureDto is null)
			{
				nomenclatureDto = _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit);
			}

			if(nomenclatureDto != null)
			{
				var allCodes = addCodesResult.Value.AllCodes;

				nomenclatureDto.Codes =
					allCodes
					.Select(_carLoadDocumentConverter.PopulateStagingTrueMarkCodes(allCodes));
			}

			var successResponse = new AddOrderCodeResponse
			{
				Nomenclature = nomenclatureDto,
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
			var pickerEmployee = GetEmployeeProxyByApiLogin(userLogin);

			var allWaterOrderItems = await GetCarLoadDocumentWaterOrderItems(orderId);
			var itemsHavingRequiredNomenclature = allWaterOrderItems.Where(item => item.Nomenclature.Id == nomenclatureId).ToList();
			var documentItemToEdit = itemsHavingRequiredNomenclature.FirstOrDefault();

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

			var isCodesCanBeAdded = _documentErrorsChecker.IsCanChangeTrueMarkCode(
				orderId,
				nomenclatureId,
				allWaterOrderItems,
				itemsHavingRequiredNomenclature,
				documentItemToEdit);

			if(isCodesCanBeAdded.IsFailure)
			{
				var error = isCodesCanBeAdded.Errors.FirstOrDefault();
				failureResponse.Error = error.Message;
				var result = Result.Failure<ChangeOrderCodeResponse>(error);
				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			StagingTrueMarkCode addedCode = null;

			if(!string.IsNullOrWhiteSpace(newScannedCode))
			{
				var changeCodesResult =
					await _codesProcessingService.ChangeStagingTrueMarkCode(
						_uow,
						newScannedCode,
						oldScannedCode,
						documentItemToEdit,
						cancellationToken);

				if(changeCodesResult.IsFailure)
				{
					var error = changeCodesResult.Errors.FirstOrDefault();
					failureResponse.Error = error.Message;
					var result = Result.Failure<ChangeOrderCodeResponse>(error);
					return RequestProcessingResult.CreateFailure(result, failureResponse);
				}

				addedCode = changeCodesResult.Value;
			}
			else
			{
				var removeCodesResult =
					await _codesProcessingService.RemoveStagingTrueMarkCode(
						_uow,
						oldScannedCode,
						documentItemToEdit.Id,
						cancellationToken);

				if(removeCodesResult.IsFailure)
				{
					var error = removeCodesResult.Errors.FirstOrDefault();
					failureResponse.Error = error.Message;
					var result = Result.Failure<ChangeOrderCodeResponse>(error);
					return RequestProcessingResult.CreateFailure(result, failureResponse);
				}
			}

			try
			{
				_uow.Commit();
			}
			catch(MySqlException mysqlException) when(mysqlException.ErrorCode == MySqlErrorCode.DuplicateKey)
			{
				_logger.LogError(mysqlException, "DuplicateEntry: {ExceptionMessage}", mysqlException.Message);

				var error = new Error("Database.Commit.Error", "Код уже был добавлен в другом документе");
				var result = Result.Failure<ChangeOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, new ChangeOrderCodeResponse
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
				var result = Result.Failure<ChangeOrderCodeResponse>(error);

				return RequestProcessingResult.CreateFailure(result, new ChangeOrderCodeResponse
				{
					Nomenclature = null,
					Result = OperationResultEnumDto.Error,
					Error = error.Message
				});
			}

			NomenclatureDto nomenclatureDto = null;

			if(nomenclatureDto is null)
			{
				nomenclatureDto = _carLoadDocumentConverter.ConvertToApiNomenclature(documentItemToEdit);
			}

			if(nomenclatureDto != null && addedCode != null)
			{
				var allCodes = addedCode.AllCodes;

				nomenclatureDto.Codes =
					allCodes
					.Select(_carLoadDocumentConverter.PopulateStagingTrueMarkCodes(allCodes));
			}

			var successResponse = new ChangeOrderCodeResponse
			{
				Nomenclature = nomenclatureDto,
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

			CreateAndSaveCarLoadDocumentLoadingProcessAction(
				carLoadDocument?.Id ?? 0, pickerEmployee,
				CarLoadDocumentLoadingProcessActionType.EndLoad);

			checkResult = _documentErrorsChecker.IsCarLoadDocumentLoadingCanBeDone(carLoadDocument, documentId);

			if(checkResult.IsFailure)
			{
				var error = checkResult.Errors.FirstOrDefault();
				failureResponse.Error = error.Message;
				var result = Result.Failure<EndLoadResponse>(error);
				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			var saveProductCodesResult =
				await _codesProcessingService.AddProductCodesToCarLoadDocumentAndDeleteStagingCodes(_uow, carLoadDocument, cancellationToken);

			if(saveProductCodesResult.IsFailure)
			{
				var error = saveProductCodesResult.Errors.FirstOrDefault();
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

			try
			{
				_uow.Commit();
			}
			catch(MySqlException mysqlException) when(mysqlException.ErrorCode == MySqlErrorCode.DuplicateKey)
			{
				_logger.LogError(mysqlException, "DuplicateEntry: {ExceptionMessage}", mysqlException.Message);

				var error = new Error("Database.Commit.Error", "Код уже был добавлен в другом документе");
				failureResponse.Error = error.Message;
				var result = Result.Failure<EndLoadResponse>(error);
				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Exception while commiting: {ExceptionMessage}", e.Message);

				var error = new Error("Database.Commit.Error", e.Message);
				failureResponse.Error = error.Message;
				var result = Result.Failure<EndLoadResponse>(error);
				return RequestProcessingResult.CreateFailure(result, failureResponse);
			}

			await PublishEdoRequestCreatedEvents(edoRequests);

			var successResponse = new EndLoadResponse
			{
				Result = OperationResultEnumDto.Success,
				Error = null
			};

			return RequestProcessingResult.CreateSuccess(Result.Success(successResponse));
		}

		private async Task<IEnumerable<CarLoadDocumentItem>> GetCarLoadDocumentWaterOrderItems(int orderId)
		{
			_logger.LogInformation("Получаем данные по заказу #{OrderId} из талона погрузки", orderId);
			var documentOrderItems =
				await _carLoadDocumentRepository.GetAccountableInTrueMarkHavingGtinItemsByCarLoadDocumentId(_uow, orderId);

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

		private bool SetDocumentLoadOperationState(
			CarLoadDocument document,
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

		private IEnumerable<PrimaryEdoRequest> CreateEdoRequests(CarLoadDocument carLoadDocument)
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

		private async Task<IDictionary<int, IEnumerable<StagingTrueMarkCode>>> GetCarLoadDocumentOrderItemsStagingCodes(
			IEnumerable<int> documentItemsIds,
			CancellationToken cancellationToken)
		{
			var carLoadDocumentItemsStagingCodes = (await _stagingTrueMarkCodeRepository.GetAsync(
				_uow,
				StagingTrueMarkCodeSpecification.CreateForRelatedDocuments(StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem, documentItemsIds.ToArray()),
				cancellationToken: cancellationToken))
				.Value
				.GroupBy(x => x.RelatedDocumentId)
				.ToDictionary(g => g.Key, g => g.Distinct());
			return carLoadDocumentItemsStagingCodes;
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
