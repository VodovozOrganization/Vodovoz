using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Responses;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.V6.Converters;
using Microsoft.Extensions.Logging;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Errors;
using Vodovoz.Extensions;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Services.TrueMark;
using Error = Vodovoz.Core.Domain.Results.Error;
using Order = Vodovoz.Domain.Orders.Order;
using OrderErrors = Vodovoz.Errors.Orders.OrderErrors;
using OrderItem = Vodovoz.Domain.Orders.OrderItem;
using OrderItemErrors = Vodovoz.Errors.Orders.OrderItemErrors;
using RouteListErrors = Vodovoz.Errors.Logistics.RouteListErrors;
using RouteListItemErrors = Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCodeErrors;
using IDomainRouteListService = Vodovoz.Services.Logistics.IRouteListService;
using Vodovoz.Tools.CallTasks;

namespace DriverAPI.Library.V6.Services
{
	internal class OrderService : IOrderService
	{
		private readonly ILogger<OrderService> _logger;
		private readonly IOrderRepository _orderRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly OrderConverter _orderConverter;
		private readonly IDriverApiSettings _driverApiSettings;
		private readonly IComplaintsRepository _complaintsRepository;
		private readonly ISmsPaymentService _aPISmsPaymentModel;
		private readonly IFastPaymentsServiceAPIHelper _fastPaymentsServiceApiHelper;
		private readonly IUnitOfWork _uow;
		private readonly QrPaymentConverter _qrPaymentConverter;
		private readonly IFastPaymentService _fastPaymentModel;
		private readonly int _maxClosingRating = 5;
		private readonly PaymentType[] _smsAndQRNotPayable = new PaymentType[] { PaymentType.PaidOnline, PaymentType.Barter, PaymentType.ContractDocumentation };
		private readonly IOrderSettings _orderSettings;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly IRouteListItemTrueMarkProductCodesProcessingService _routeListItemTrueMarkProductCodesProcessingService;
		private readonly IGenericRepository<CarLoadDocument> _carLoadDocumentRepository;
		private readonly IOrderContractUpdater _contractUpdater;
		private readonly ICounterpartyEdoAccountController _edoAccountController;
		private readonly IDomainRouteListService _domainRouteListService;
		private readonly ICallTaskWorker _callTaskWorker;

		public OrderService(
			ILogger<OrderService> logger,
			IOrderRepository orderRepository,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			OrderConverter orderConverter,
			IDriverApiSettings driverApiSettings,
			IComplaintsRepository complaintsRepository,
			ISmsPaymentService aPISmsPaymentModel,
			IFastPaymentsServiceAPIHelper fastPaymentsServiceApiHelper,
			IUnitOfWork unitOfWork,
			QrPaymentConverter qrPaymentConverter,
			IFastPaymentService fastPaymentModel,
			IOrderSettings orderSettings,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			IRouteListItemTrueMarkProductCodesProcessingService routeListItemTrueMarkProductCodesProcessingService,
			IGenericRepository<CarLoadDocument> carLoadDocumentRepository,
			IOrderContractUpdater contractUpdater,
			ICounterpartyEdoAccountController edoAccountController,
			IDomainRouteListService domainRouteListService,
			ICallTaskWorker callTaskWorker
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_orderConverter = orderConverter ?? throw new ArgumentNullException(nameof(orderConverter));
			_driverApiSettings = driverApiSettings ?? throw new ArgumentNullException(nameof(driverApiSettings));
			_complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			_aPISmsPaymentModel = aPISmsPaymentModel ?? throw new ArgumentNullException(nameof(aPISmsPaymentModel));
			_fastPaymentsServiceApiHelper = fastPaymentsServiceApiHelper ?? throw new ArgumentNullException(nameof(fastPaymentsServiceApiHelper));
			_uow = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_qrPaymentConverter = qrPaymentConverter ?? throw new ArgumentNullException(nameof(qrPaymentConverter));
			_fastPaymentModel = fastPaymentModel ?? throw new ArgumentNullException(nameof(fastPaymentModel));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_routeListItemTrueMarkProductCodesProcessingService = routeListItemTrueMarkProductCodesProcessingService ?? throw new ArgumentNullException(nameof(routeListItemTrueMarkProductCodesProcessingService));
			_carLoadDocumentRepository = carLoadDocumentRepository ?? throw new ArgumentNullException(nameof(carLoadDocumentRepository));
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));
			_edoAccountController = edoAccountController ?? throw new ArgumentNullException(nameof(edoAccountController));
			_domainRouteListService = domainRouteListService ?? throw new ArgumentNullException(nameof(domainRouteListService));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
		}

		/// <summary>
		/// Получение заказа в требуемом формате из заказа программы ДВ (использует функцию ниже)
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>APIOrder</returns>
		public Result<OrderDto> GetOrder(int orderId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				return Result.Failure<OrderDto>(OrderErrors.NotFound);
			}

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(_uow, vodovozOrder);

			if(routeListItem is null)
			{
				return Result.Failure<OrderDto>(RouteListItemErrors.NotFoundAssociatedWithOrder);
			}

			var order = _orderConverter.ConvertToAPIOrder(
				vodovozOrder,
				routeListItem,
				_aPISmsPaymentModel.GetOrderSmsPaymentStatus(orderId),
				_fastPaymentModel.GetOrderFastPaymentStatus(orderId, vodovozOrder.OnlinePaymentNumber));

			var additionalInfo = GetAdditionalInfo(vodovozOrder);

			if(additionalInfo.IsFailure)
			{
				return Result.Failure<OrderDto>(additionalInfo.Errors);
			}

			order.OrderAdditionalInfo = GetAdditionalInfo(vodovozOrder).Value;

			return order;
		}

		/// <summary>
		/// Получение списка заказов в требуемом формате из заказов программы ДВ по списку идентификаторов
		/// </summary>
		/// <param name="orderIds">Список идентификаторов заказов</param>
		/// <returns>IEnumerable APIOrder</returns>
		public IEnumerable<OrderDto> Get(int[] orderIds)
		{
			var result = new List<OrderDto>();
			var vodovozOrders = _orderRepository.GetOrders(_uow, orderIds).ToArray();

			var carLoadDocuments = _carLoadDocumentRepository.Get(_uow, x => x.RouteList.Addresses.Any(routeListItem => orderIds.Contains(routeListItem.Order.Id)));

			foreach(var vodovozOrder in vodovozOrders)
			{
				var smsPaymentStatus = _aPISmsPaymentModel.GetOrderSmsPaymentStatus(vodovozOrder.Id);
				var qrPaymentStatus = _fastPaymentModel.GetOrderFastPaymentStatus(vodovozOrder.Id, vodovozOrder.OnlinePaymentNumber);
				var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(_uow, vodovozOrder);
				var order = _orderConverter.ConvertToAPIOrder(vodovozOrder, routeListItem, smsPaymentStatus, qrPaymentStatus);
				order.OrderAdditionalInfo = GetAdditionalInfo(vodovozOrder).Value;
				result.Add(order);
			}

			return result;
		}

		/// <summary>
		/// Получение типов оплаты на которые можно изменить тип оплаты заказа переданного в аргументе
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>IEnumerable APIPaymentType</returns>
		public Result<IEnumerable<PaymentDtoType>> GetAvailableToChangePaymentTypes(int orderId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				return Result.Failure<IEnumerable<PaymentDtoType>>(OrderErrors.NotFound);
			}

			return Result.Success(GetAvailableToChangePaymentTypes(vodovozOrder));
		}

		/// <summary>
		/// Получение типов оплаты на которые можно изменить тип оплаты заказа переданного в аргументе
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <returns>IEnumerable<see cref="PaymentDtoType"/></returns>
		public IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(Order order)
		{
			var availablePaymentTypes = new List<PaymentDtoType>();

			bool paid = _fastPaymentModel.GetOrderFastPaymentStatus(order.Id) == FastPaymentStatus.Performed;

			if(order.PaymentType == PaymentType.Cash)
			{
				availablePaymentTypes.Add(PaymentDtoType.TerminalCard);
				availablePaymentTypes.Add(PaymentDtoType.TerminalQR);
				availablePaymentTypes.Add(PaymentDtoType.DriverApplicationQR);
			}

			if(order.PaymentType == PaymentType.Terminal)
			{
				if(order.PaymentByTerminalSource == PaymentByTerminalSource.ByQR)
				{
					availablePaymentTypes.Add(PaymentDtoType.TerminalCard);
				}
				else
				{
					availablePaymentTypes.Add(PaymentDtoType.TerminalQR);
				}

				availablePaymentTypes.Add(PaymentDtoType.Cash);
				availablePaymentTypes.Add(PaymentDtoType.DriverApplicationQR);
			}

			if(order.PaymentType == PaymentType.DriverApplicationQR
				|| order.PaymentType == PaymentType.SmsQR && !paid)
			{
				availablePaymentTypes.Add(PaymentDtoType.Cash);
				availablePaymentTypes.Add(PaymentDtoType.TerminalCard);
				availablePaymentTypes.Add(PaymentDtoType.TerminalQR);
			}

			return availablePaymentTypes;
		}

		/// <summary>
		/// Получение дополнительной информации для заказа по идентификатору
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>APIOrderAdditionalInfo</returns>
		public Result<OrderAdditionalInfoDto> GetAdditionalInfo(int orderId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				return Result.Failure<OrderAdditionalInfoDto>(OrderErrors.NotFound);
			}

			return GetAdditionalInfo(vodovozOrder);
		}

		/// <summary>
		/// Получение дополнительной информации для заказа из заказа программы ДВ
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <returns>APIOrderAdditionalInfo</returns>
		public Result<OrderAdditionalInfoDto> GetAdditionalInfo(Order order)
		{
			return new OrderAdditionalInfoDto
			{
				AvailablePaymentTypes = GetAvailableToChangePaymentTypes(order),
				CanSendSms = CanSendSmsForPayment(order, _aPISmsPaymentModel.GetOrderSmsPaymentStatus(order.Id)),
				CanReceiveQRCode = CanReceiveQRCodeForPayment(order),
			};
		}

		public Result ChangeOrderPaymentType(int orderId, PaymentType paymentType, Employee driver, PaymentByTerminalSource? paymentByTerminalSource)
		{
			if(driver is null)
			{
				throw new ArgumentNullException(nameof(driver));
			}

			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);

				return Result.Failure(Vodovoz.Errors.Orders.OrderErrors.NotFound);
			}

			if(vodovozOrder.OrderStatus != OrderStatus.OnTheWay)
			{
				_logger.LogWarning("Нельзя изменить тип оплаты для заказа: {OrderId}, заказ не в статусе {OrderStatus}.",
					orderId,
					OrderStatus.OnTheWay.GetEnumDisplayName());

				return Result.Failure(OrderErrors.NotInOnTheWayStatus);
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);

				return Result.Failure(Vodovoz.Errors.Logistics.RouteListErrors.NotFoundAssociatedWithOrder);
			}

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning("Сотрудник {EmployeeId} попытался сменить тип оплаты заказа {OrderId} водителя {DriverId}",
					driver.Id,
					orderId,
					routeList.Driver.Id);

				return Result.Failure(Errors.Security.Authorization.OrderAccessDenied);
			}

			vodovozOrder.UpdatePaymentType(paymentType, _contractUpdater);
			vodovozOrder.PaymentByTerminalSource = paymentByTerminalSource;

			_uow.Save(vodovozOrder);
			_uow.Commit();

			return Result.Success();
		}

		public async Task<Result> CompleteOrderDelivery(
			DateTime actionTime,
			Employee driver,
			IDriverOrderShipmentInfo completeOrderInfo,
			IDriverComplaintInfo driverComplaintInfo)
		{
			var orderId = completeOrderInfo.OrderId;
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				return Result.Failure(OrderErrors.NotFound);
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(RouteListErrors.NotFoundAssociatedWithOrder);
			}

			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(RouteListItemErrors.NotFoundAssociatedWithOrder);
			}

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning("Сотрудник {EmployeeId} попытался завершить заказ {OrderId} водителя {DriverId}",
					driver.Id, orderId, routeList.Driver.Id);
				return Result.Failure(Errors.Security.Authorization.OrderAccessDenied);
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя завершить заказ: {OrderId}, МЛ не в пути", orderId);
				return Result.Failure<PayByQrResponse>(RouteListErrors.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя завершить заказ: {OrderId}, адрес МЛ {RouteListAddressId} не в пути", orderId, routeListAddress.Id);
				return Result.Failure<PayByQrResponse>(RouteListItemErrors.NotEnRouteState);
			}

			var trueMarkCodesProcessResult = await ProcessScannedCodes(completeOrderInfo, routeListAddress);

			if(trueMarkCodesProcessResult.IsFailure)
			{
				return trueMarkCodesProcessResult;
			}

			routeListAddress.DriverBottlesReturned = completeOrderInfo.BottlesReturnCount;
			
			_domainRouteListService.ChangeAddressStatus(_uow, routeList, routeListAddress.Id, RouteListItemStatus.Completed, _callTaskWorker);

			CreateComplaintIfNeeded(driverComplaintInfo, vodovozOrder, driver, actionTime);

			if(completeOrderInfo.BottlesReturnCount != vodovozOrder.BottlesReturn)
			{
				if(!string.IsNullOrWhiteSpace(completeOrderInfo.DriverComment))
				{
					vodovozOrder.DriverMobileAppComment = completeOrderInfo.DriverComment;
					vodovozOrder.DriverMobileAppCommentTime = actionTime;
				}

				vodovozOrder.DriverCallType = DriverCallType.CommentFromMobileApp;

				await _uow.SaveAsync(vodovozOrder);
			}

			await _uow.SaveAsync(routeListAddress);
			await _uow.SaveAsync(routeList);

			var edoRequest = _uow.Session.Query<PrimaryEdoRequest>()
				.Where(x => x.Order.Id == vodovozOrder.Id)
				.Take(1)
				.SingleOrDefault();

			var isAllOwnNeedsOrderDriversScannedCodesProcessed =
				vodovozOrder.Client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds
				&& await _orderRepository.IsAllDriversScannedCodesInOrderProcessed(_uow, vodovozOrder.Id);

			var edoRequestCreated = false;
			if((!vodovozOrder.IsNeedIndividualSetOnLoad(_edoAccountController) && !vodovozOrder.IsNeedIndividualSetOnLoadForTender)
				&& edoRequest == null
				&& (vodovozOrder.Client.ReasonForLeaving != ReasonForLeaving.ForOwnNeeds || isAllOwnNeedsOrderDriversScannedCodesProcessed))
			{
				edoRequest = CreateEdoRequests(vodovozOrder, routeListAddress);
				edoRequestCreated = true;
			}

			if(edoRequestCreated)
			{
				return Result.Success(edoRequest.Id);
			}

			return Result.Success();
		}

		private PrimaryEdoRequest CreateEdoRequests(Order vodovozOrder, RouteListItem routeListAddress)
		{
			var edoRequest = new PrimaryEdoRequest
			{
				Time = DateTime.Now,
				Source = CustomerEdoRequestSource.Driver,
				DocumentType = EdoDocumentType.UPD,
				Order = vodovozOrder,
			};

			foreach(var code in routeListAddress.TrueMarkCodes)
			{
				edoRequest.ProductCodes.Add(code);
			}

			_uow.Save(edoRequest);

			return edoRequest;
		}

		public async Task<Result> UpdateOrderShipmentInfoAsync(
			DateTime actionTime,
			Employee driver,
			IDriverOrderShipmentInfo completeOrderInfo)
		{
			var orderId = completeOrderInfo.OrderId;
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				return Result.Failure(OrderErrors.NotFound);
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(RouteListErrors.NotFoundAssociatedWithOrder);
			}

			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(RouteListItemErrors.NotFoundAssociatedWithOrder);
			}

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning("Сотрудник {EmployeeId} попытался изменить заказ {OrderId} водителя {DriverId}",
					driver.Id, orderId, routeList.Driver.Id);
				return Result.Failure<PayByQrResponse>(Errors.Security.Authorization.OrderAccessDenied);
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя завершить заказ: {OrderId}, МЛ не в пути", orderId);
				return Result.Failure<PayByQrResponse>(RouteListErrors.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя завершить заказ: {OrderId}, адрес МЛ {RouteListAddressId} не в пути", orderId, routeListAddress.Id);
				return Result.Failure<PayByQrResponse>(RouteListItemErrors.NotEnRouteState);
			}

			var trueMarkCodesProcessResult =
				await ProcessScannedCodes(completeOrderInfo, routeListAddress);

			if(trueMarkCodesProcessResult.IsFailure)
			{
				return trueMarkCodesProcessResult;
			}

			routeListAddress.DriverBottlesReturned = completeOrderInfo.BottlesReturnCount;

			if(completeOrderInfo.BottlesReturnCount != vodovozOrder.BottlesReturn)
			{
				if(!string.IsNullOrWhiteSpace(completeOrderInfo.DriverComment))
				{
					vodovozOrder.DriverMobileAppComment = completeOrderInfo.DriverComment;
					vodovozOrder.DriverMobileAppCommentTime = actionTime;
				}

				vodovozOrder.DriverCallType = DriverCallType.CommentFromMobileApp;

				await _uow.SaveAsync(vodovozOrder);
			}

			await _uow.SaveAsync(routeListAddress);
			await _uow.SaveAsync(routeList);

			await _uow.CommitAsync();

			return Result.Success();
		}

		public async Task<Result<PayByQrResponse>> SendQrPaymentRequestAsync(int orderId, int driverId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				return Result.Failure<PayByQrResponse>(OrderErrors.NotFound);
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList is null)
			{
				return Result.Failure<PayByQrResponse>(RouteListErrors.NotFoundAssociatedWithOrder);
			}

			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(routeListAddress is null)
			{
				return Result.Failure<PayByQrResponse>(RouteListItemErrors.NotFoundAssociatedWithOrder);
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				return Result.Failure<PayByQrResponse>(RouteListErrors.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				return Result.Failure<PayByQrResponse>(RouteListItemErrors.NotEnRouteState);
			}

			if(routeList.Driver.Id != driverId)
			{
				_logger.LogWarning("Сотрудник {EmployeeId} попытался запросить оплату по QR для заказа {OrderId} водителя {DriverId}",
					driverId, orderId, routeList.Driver.Id);

				return Result.Failure<PayByQrResponse>(Errors.Security.Authorization.RouteListAccessDenied);
			}

			var qrResponseDto = await _fastPaymentsServiceApiHelper.SendPaymentAsync(orderId);
			var payByQRResponseDto = _qrPaymentConverter.ConvertToPayByQRResponseDto(qrResponseDto);

			if(payByQRResponseDto.QRPaymentStatus == QrPaymentDtoStatus.Paid)
			{
				payByQRResponseDto.AvailablePaymentTypes = Enumerable.Empty<PaymentDtoType>();
				payByQRResponseDto.CanReceiveQR = false;
			}
			else
			{
				var availableToChangePaymentTypesResult = GetAvailableToChangePaymentTypes(orderId);

				if(availableToChangePaymentTypesResult.IsFailure)
				{
					return Result.Failure<PayByQrResponse>(availableToChangePaymentTypesResult.Errors);
				}

				payByQRResponseDto.AvailablePaymentTypes = availableToChangePaymentTypesResult.Value;
				payByQRResponseDto.CanReceiveQR = true;
			}

			return payByQRResponseDto;
		}

		public Result UpdateBottlesByStockActualCount(int orderId, int bottlesByStockActualCount)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				return Result.Failure(OrderErrors.NotFound);
			}

			if(!vodovozOrder.IsBottleStock)
			{
				return Result.Success();
			}

			if(vodovozOrder.BottlesByStockActualCount == bottlesByStockActualCount)
			{
				return Result.Success();
			}

			vodovozOrder.IsBottleStockDiscrepancy = vodovozOrder.BottlesByStockCount != bottlesByStockActualCount;

			vodovozOrder.BottlesByStockActualCount = bottlesByStockActualCount;
			vodovozOrder.CalculateBottlesStockDiscounts(_orderSettings, true);

			_uow.Save(vodovozOrder);
			_uow.Commit();

			return Result.Success();
		}

		private async Task<Result> ProcessScannedCodes(
			IDriverOrderShipmentInfo completeOrderInfo,
			RouteListItem routeListAddress)
		{
			if(routeListAddress.Order.IsNeedIndividualSetOnLoad(_edoAccountController)
				|| routeListAddress.Order.IsNeedIndividualSetOnLoadForTender)
			{
				return CheckNetworkClientOrderScannedCodes(routeListAddress);
			}

			if(routeListAddress.Order.IsOrderForResale || routeListAddress.Order.IsOrderForTender)
			{
				return CheckResaleOrderScannedCodes(routeListAddress);
			}

			return await ProcessOwnUseOrderScannedCodesAsync(completeOrderInfo, routeListAddress);
		}

		private async Task<Result> ProcessOwnUseOrderScannedCodesAsync(
			IDriverOrderShipmentInfo completeOrderInfo,
			RouteListItem routeListAddress)
		{
			routeListAddress.UnscannedCodesReason = completeOrderInfo.UnscannedCodesReason;

			var driversScannedCodes = new List<DriversScannedTrueMarkCode>();

			foreach(var scannedItem in completeOrderInfo.ScannedItems)
			{
				var bottleCodes = scannedItem.BottleCodes
					.Distinct()
					.Select(x => new DriversScannedTrueMarkCode
					{
						RawCode = x,
						OrderItemId = scannedItem.OrderSaleItemId,
						RouteListAddressId = routeListAddress.Id,
						IsDefective = false
					})
					.ToArray();

				var defectiveCodes = scannedItem.DefectiveBottleCodes
					.Distinct()
					.Select(x => new DriversScannedTrueMarkCode
					{
						RawCode = x,
						OrderItemId = scannedItem.OrderSaleItemId,
						RouteListAddressId = routeListAddress.Id,
						IsDefective = true
					})
					.ToArray();

				driversScannedCodes.AddRange(bottleCodes);
				driversScannedCodes.AddRange(defectiveCodes);
			}

			foreach(var scannedCode in driversScannedCodes)
			{
				await _uow.SaveAsync(scannedCode);
			}

			return Result.Success();
		}

		private Result CheckResaleOrderScannedCodes(RouteListItem routeListAddress)
		{
			return IsAllRouteListItemTrueMarkProductCodesAddedToOrder(routeListAddress.Order.Id);
		}

		private Result IsAllRouteListItemTrueMarkProductCodesAddedToOrder(int orderId)
		{
			var isAllTrueMarkCodesAdded =
				_orderRepository.IsAllRouteListItemTrueMarkProductCodesAddedToOrder(_uow, orderId);

			if(!isAllTrueMarkCodesAdded)
			{
				return Result.Failure(TrueMarkCodeErrors.NotAllCodesAdded);
			}

			return Result.Success();
		}

		private Result CheckNetworkClientOrderScannedCodes(RouteListItem routeListAddress)
		{
			return IsAllCarLoadDocumentItemTrueMarkProductCodesAddedToOrder(routeListAddress.Order.Id);
		}

		private Result IsAllCarLoadDocumentItemTrueMarkProductCodesAddedToOrder(int orderId)
		{
			var isCarLoadDocumentLoadOperationStateDone =
				_orderRepository.IsOrderCarLoadDocumentLoadOperationStateDone(_uow, orderId);

			if(!isCarLoadDocumentLoadOperationStateDone)
			{
				return Result.Failure(TrueMarkCodeErrors.NotAllCodesAdded);
			}

			return Result.Success();
		}

		private void CreateComplaintIfNeeded(IDriverComplaintInfo driverComplaintInfo, Order order, Employee driver, DateTime actionTime)
		{
			if(driverComplaintInfo.Rating < _maxClosingRating)
			{
				var complaintReason = _complaintsRepository.GetDriverComplaintReasonById(_uow, driverComplaintInfo.DriverComplaintReasonId);
				var complaintSource = _complaintsRepository.GetComplaintSourceById(_uow, _driverApiSettings.ComplaintSourceId);
				var reason = complaintReason?.Name ?? driverComplaintInfo.OtherDriverComplaintReasonComment;

				var complaint = new Complaint
				{
					ComplaintSource = complaintSource,
					ComplaintType = ComplaintType.Driver,
					Order = order,
					DriverRating = driverComplaintInfo.Rating,
					DeliveryPoint = order.DeliveryPoint,
					CreationDate = actionTime,
					ChangedDate = actionTime,
					Driver = driver,
					CreatedBy = driver,
					ChangedBy = driver,
					ComplaintText = $"Заказ номер {order.Id}\n" +
						$"По причине {reason}"
				};

				complaint.SetStatus(ComplaintStatuses.InProcess);

				_uow.Save(complaint);
			}
		}

		/// <summary>
		/// Проверка возможности отправки СМС для оплаты
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <param name="smsPaymentStatus">Статус оплаты СМС</param>
		/// <returns></returns>
		private bool CanSendSmsForPayment(Order order, SmsPaymentStatus? smsPaymentStatus)
		{
			return !_smsAndQRNotPayable.Contains(order.PaymentType) && order.OrderSum > 0;
		}

		/// <summary>
		/// Проверка возможности отправки QR-кода для оплаты
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <returns></returns>
		private bool CanReceiveQRCodeForPayment(Order order)
		{
			return !_smsAndQRNotPayable.Contains(order.PaymentType) && order.OrderSum > 0;
		}

		public async Task<RequestProcessingResult<TrueMarkCodeProcessingResultResponse>> AddTrueMarkCode(
			DateTime actionTime,
			Employee driver,
			int orderId,
			int orderSaleItemId,
			string scannedCode,
			CancellationToken cancellationToken)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				return GetFailureTrueMarkCodeProcessingResponse(OrderErrors.NotFound, errorMessage: $"Заказ не найден: {orderId}");
			}

			var vodovozOrderItem = vodovozOrder.OrderItems.FirstOrDefault(x => x.Id == orderSaleItemId);

			if(vodovozOrderItem is null)
			{
				_logger.LogWarning("Строка заказа не найдена: {OrderItemId}", orderSaleItemId);
				return GetFailureTrueMarkCodeProcessingResponse(OrderItemErrors.NotFound, errorMessage: $"Строка заказа не найдена: {orderSaleItemId}");
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListErrors.NotFoundAssociatedWithOrder, errorMessage: $"МЛ для заказа: {orderId} не найден");
			}

			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ для заказа: {OrderId} не найден", orderId);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListItemErrors.NotFoundAssociatedWithOrder, errorMessage: $"Адрес МЛ для заказа: {orderId} не найден");
			}

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning("Сотрудник {DriverId} попытался добавить код в заказ {OrderId} водителя {RouteListAssignedToDriverId}",
					driver.Id,
					orderId,
					routeList.Driver.Id);
				return GetFailureTrueMarkCodeProcessingResponse(Errors.Security.Authorization.OrderAccessDenied, errorMessage: $"Сотрудник {driver.Id} попытался добавить код в заказ {orderId} водителя {routeList.Driver.Id}");
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя добавить код к заказу {OrderId}, МЛ {RouteListId} не в пути", orderId, routeList.Id);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListErrors.NotEnRouteState, vodovozOrderItem, routeListAddress, $"Нельзя добавить код к заказу {orderId}, МЛ {routeList.Id} не в пути");
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя добавить код к заказу {OrderId}, адрес МЛ {RouteListAddressId} не в пути", orderId, routeListAddress.Id);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListItemErrors.NotEnRouteState, vodovozOrderItem, routeListAddress, $"Нельзя добавить код к заказу {orderId}, адрес МЛ {routeListAddress.Id} не в пути");
			}

			// Если на скаладе не сканировались коды ЧЗ, то разрешить добавить коды

			var carLoadDocuments = _carLoadDocumentRepository.Get(_uow, x => x.RouteList.Addresses.Any(routeListItem => routeListItem.Order.Id == orderId));

			var carLoadDocumentItems = carLoadDocuments.SelectMany(x => x.Items.Where(x => x.OrderId == vodovozOrder.Id));

			var hasCodesInCarLoadDocument = carLoadDocumentItems.Any(x => x.TrueMarkCodes.Any(x => x.SourceCode != null || x.ResultCode != null));

			if(vodovozOrderItem.IsTrueMarkCodesMustBeAddedInWarehouse(_edoAccountController) && hasCodesInCarLoadDocument)
			{
				_logger.LogWarning("Коды ЧЗ сетевого, либо госзаказа {OrderId} должны добавляться на складе", orderId);
				return GetFailureTrueMarkCodeProcessingResponse(TrueMarkCodeErrors.TrueMarkCodesHaveToBeAddedInWarehouse, vodovozOrderItem, routeListAddress, $"Коды ЧЗ сетевого заказа {orderId} должны добавляться на складе");
			}

			var result = await AddTrueMarkCodeToRouteListItemWithCodeChecking(
				_uow,
				routeListAddress,
				vodovozOrderItem,
				scannedCode,
				SourceProductCodeStatus.Accepted,
				cancellationToken);

			try
			{
				_uow.Commit();

				return result;
			}
			catch(Exception e)
			{
				_uow.Session.GetCurrentTransaction()?.Rollback();
				_logger.LogError(e, "Exception while commiting: {ExceptionMessage}", e.Message);
				throw;
			}
		}

		public async Task<RequestProcessingResult<TrueMarkCodeProcessingResultResponse>> ChangeTrueMarkCode(
			DateTime actionTime,
			Employee driver,
			int orderId,
			int orderSaleItemId,
			string oldScannedCode,
			string newScannedCode,
			CancellationToken cancellationToken)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				return GetFailureTrueMarkCodeProcessingResponse(OrderErrors.NotFound, errorMessage: $"Заказ не найден: {orderId}");
			}

			var vodovozOrderItem = vodovozOrder.OrderItems.FirstOrDefault(x => x.Id == orderSaleItemId);

			if(vodovozOrderItem is null)
			{
				_logger.LogWarning("Строка заказа не найдена: {OrderItemId}", orderSaleItemId);
				return GetFailureTrueMarkCodeProcessingResponse(OrderItemErrors.NotFound, errorMessage: $"Строка заказа не найдена: {orderSaleItemId}");
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListErrors.NotFoundAssociatedWithOrder, errorMessage: $"МЛ для заказа: {orderId} не найден");
			}

			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ для заказа: {OrderId} не найден", orderId);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListItemErrors.NotFoundAssociatedWithOrder, errorMessage: $"Адрес МЛ для заказа: {orderId} не найден");
			}

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning(
					"Сотрудник {DriverId} попытался заменить код в заказе {OrderId} водителя {RouteListAssignedToDriverId}",
					driver.Id,
					orderId,
					routeList.Driver.Id);
				return GetFailureTrueMarkCodeProcessingResponse(Errors.Security.Authorization.OrderAccessDenied, errorMessage: $"Сотрудник {driver.Id} попытался заменить код в заказе {orderId} водителя {routeList.Driver.Id}");
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя заменить код в заказе {OrderId}, МЛ {RouteListId} не в пути", orderId, routeList.Id);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListErrors.NotEnRouteState, vodovozOrderItem, routeListAddress, $"Нельзя заменить код в заказе {orderId}, МЛ {routeList.Id} не в пути");
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя заменить код в заказе {OrderId}, адрес МЛ {RouteListAddressId} не в пути", orderId, routeListAddress.Id);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListItemErrors.NotEnRouteState, vodovozOrderItem, routeListAddress, $"Нельзя заменить код в заказе {orderId}, адрес МЛ {routeListAddress.Id} не в пути");
			}

			return await ChangeTrueMarkCodeToRouteListItemWithCodeChecking(
				routeListAddress,
				vodovozOrderItem,
				oldScannedCode,
				newScannedCode,
				SourceProductCodeStatus.Accepted,
				cancellationToken);
		}

		public async Task<RequestProcessingResult<TrueMarkCodeProcessingResultResponse>> RemoveTrueMarkCode(
			Employee driver,
			int orderId,
			int orderSaleItemId,
			string scannedCode,
			CancellationToken cancellationToken)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				return GetFailureTrueMarkCodeProcessingResponse(OrderErrors.NotFound, errorMessage: $"Заказ не найден: {orderId}");
			}

			var vodovozOrderItem = vodovozOrder.OrderItems.FirstOrDefault(x => x.Id == orderSaleItemId);

			if(vodovozOrderItem is null)
			{
				_logger.LogWarning("Строка заказа не найдена: {OrderItemId}", orderSaleItemId);
				return GetFailureTrueMarkCodeProcessingResponse(OrderItemErrors.NotFound, errorMessage: $"Строка заказа не найдена: {orderSaleItemId}");
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListErrors.NotFoundAssociatedWithOrder, errorMessage: $"МЛ для заказа: {orderId} не найден");
			}

			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ для заказа: {OrderId} не найден", orderId);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListItemErrors.NotFoundAssociatedWithOrder, errorMessage: $"Адрес МЛ для заказа: {orderId} не найден");
			}

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning("Сотрудник {DriverId} попытался удалить код в заказе {OrderId} водителя {RouteListDriverId}", driver.Id, orderId, routeList.Driver.Id);
				return GetFailureTrueMarkCodeProcessingResponse(Errors.Security.Authorization.OrderAccessDenied, errorMessage: $"Сотрудник {driver.Id} попытался удалить код в заказе {orderId} водителя {routeList.Driver.Id}");
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя удалить код из заказа {OrderId}, МЛ {RouteListId} не в пути", orderId, routeList.Id);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListErrors.NotEnRouteState, vodovozOrderItem, routeListAddress, $"Нельзя удалить код из заказа {orderId}, МЛ {routeList.Id} не в пути");
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя удалить код из заказа {OrderId}, адрес МЛ {RouteListAddressId} не в пути", orderId, routeListAddress.Id);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListItemErrors.NotEnRouteState, vodovozOrderItem, routeListAddress, $"Нельзя удалить код из заказа {orderId}, адрес МЛ {routeListAddress.Id} не в пути");
			}

			var oldTrueMarkCodeResult = await _trueMarkWaterCodeService.GetTrueMarkCodeByScannedCode(_uow, scannedCode, cancellationToken);

			if(oldTrueMarkCodeResult.IsFailure)
			{
				var error = oldTrueMarkCodeResult.Errors.FirstOrDefault();
				var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);
				return RequestProcessingResult.CreateFailure(result, new TrueMarkCodeProcessingResultResponse
				{
					Nomenclature = null,
					Result = RequestProcessingResultTypeDto.Error,
					Error = error.Message
				});
			}

			IEnumerable<TrueMarkAnyCode> oldTrueMarkAnyCodes = oldTrueMarkCodeResult.Value.Match(
				transportCode => transportCode.GetAllCodes(),
				groupCode => groupCode.GetAllCodes(),
				waterCode => new TrueMarkAnyCode[] { waterCode })
				.ToArray();

			_uow.Commit();
			_uow.Session.BeginTransaction();

			if(oldTrueMarkCodeResult.Value.Match(
				transportCode => transportCode.ParentTransportCodeId != null,
				groupCode => groupCode.ParentTransportCodeId != null
					|| groupCode.ParentWaterGroupCodeId != null,
				waterCode => waterCode.ParentTransportCodeId != null
					|| waterCode.ParentWaterGroupCodeId != null))
			{
				var error = Errors.TrueMark.AggregatedCodeRemovalAttempt;
				var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);
				return RequestProcessingResult.CreateFailure(result, new TrueMarkCodeProcessingResultResponse
				{
					Nomenclature = null,
					Result = RequestProcessingResultTypeDto.Error,
					Error = error.Message
				});
			}

			foreach(var codeToRemove in oldTrueMarkAnyCodes)
			{
				if(!codeToRemove.IsTrueMarkWaterIdentificationCode)
				{
					continue;
				}

				var result = RemoveTrueMarkCodeFromRouteListItem(
					_uow,
					routeListAddress,
					vodovozOrderItem.Id,
					codeToRemove.TrueMarkWaterIdentificationCode);

				if(result.IsFailure)
				{
					return RequestProcessingResult.CreateFailure(
						Result.Failure<TrueMarkCodeProcessingResultResponse>(result.Errors),
						new TrueMarkCodeProcessingResultResponse
						{
							Nomenclature = null,
							Result = RequestProcessingResultTypeDto.Error,
							Error = $"Не удалось удалить код: {string.Join(", ", result.Errors.Select(x => x.Message))}",
						});
				}
			}

			foreach(var oldCodeToRemoveFromDatabase in oldTrueMarkAnyCodes)
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
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Exception while commiting: {ExceptionMessage}", e.Message);
			}

			foreach(var oldCodeToRemoveFromDatabase in oldTrueMarkAnyCodes)
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
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Exception while commiting: {ExceptionMessage}", e.Message);
			}

			var successResponse = new TrueMarkCodeProcessingResultResponse
			{
				Nomenclature = null,
				Result = RequestProcessingResultTypeDto.Success,
				Error = null
			};

			return RequestProcessingResult.CreateSuccess(Result.Success(successResponse));
		}

		private RequestProcessingResult<TrueMarkCodeProcessingResultResponse> GetFailureTrueMarkCodeProcessingResponse(
			Error error,
			OrderItem orderItem = null,
			RouteListItem routeListAddress = null,
			string errorMessage = default)
		{
			var response = new TrueMarkCodeProcessingResultResponse
			{
				Result = RequestProcessingResultTypeDto.Error,
				Error = string.IsNullOrWhiteSpace(errorMessage) ? error.Message : errorMessage
			};

			if(orderItem != null && routeListAddress != null)
			{
				response.Nomenclature = _orderConverter.ConvertOrderItemTrueMarkCodesDataToDto(orderItem, routeListAddress);
			}

			var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);

			return RequestProcessingResult.CreateFailure(result, response);
		}

		public async Task<RequestProcessingResult<TrueMarkCodeProcessingResultResponse>> AddTrueMarkCodeToRouteListItemWithCodeChecking(
			IUnitOfWork uow,
			RouteListItem routeListAddress,
			OrderItem vodovozOrderItem,
			string scannedCode,
			SourceProductCodeStatus status,
			CancellationToken cancellationToken,
			bool isCheckForCodeChange = false)
		{
			var trueMarkCodeResult =
				await _trueMarkWaterCodeService.GetTrueMarkCodeByScannedCode(uow, scannedCode, cancellationToken);

			if(trueMarkCodeResult.IsFailure)
			{
				var error = trueMarkCodeResult.Errors.FirstOrDefault();

				var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);

				return RequestProcessingResult.CreateFailure(result, new TrueMarkCodeProcessingResultResponse
				{
					Nomenclature = null,
					Result = RequestProcessingResultTypeDto.Error,
					Error = error.Message
				});
			}

			var aggregationValidationResult = _routeListItemTrueMarkProductCodesProcessingService.ValidateTrueMarkCodeIsInAggregationCode(trueMarkCodeResult.Value);

			if(aggregationValidationResult.IsFailure)
			{
				var error = aggregationValidationResult.Errors.FirstOrDefault();

				var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);

				return RequestProcessingResult.CreateFailure(result, new TrueMarkCodeProcessingResultResponse
				{
					Nomenclature = null,
					Result = RequestProcessingResultTypeDto.Error,
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

				var error = Errors.UnitOfWork.CommitError;
				var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);

				return RequestProcessingResult.CreateFailure(result, new TrueMarkCodeProcessingResultResponse
				{
					Nomenclature = null,
					Result = RequestProcessingResultTypeDto.Error,
					Error = error.Message
				});
			}

			_uow.Commit();
			_uow.Session.BeginTransaction();

			IEnumerable<TrueMarkAnyCode> trueMarkAnyCodes = trueMarkCodeResult.Value.Match(
				transportCode => trueMarkAnyCodes = transportCode.GetAllCodes(),
				groupCode => trueMarkAnyCodes = groupCode.GetAllCodes(),
				waterCode => new TrueMarkAnyCode[] { waterCode });

			var newIdentificationCodes = trueMarkAnyCodes
				.Where(x => x.IsTrueMarkWaterIdentificationCode)
				.Select(x => x.TrueMarkWaterIdentificationCode)
				.ToArray();

			var newIdentificationCodesIds = newIdentificationCodes.Select(x => x.Id).ToArray();

			var newCodesUsedInResult = _uow.Session.Query<CarLoadDocumentItemTrueMarkProductCode>()
				.Where(x => newIdentificationCodesIds
					.Contains(x.ResultCode.Id))
				.ToArray();

			var newCodesUsedInSource = _uow.Session.Query<CarLoadDocumentItemTrueMarkProductCode>()
				.Where(x => newIdentificationCodesIds
					.Contains(x.SourceCode.Id))
				.ToArray();

			var newCodesUsed = newCodesUsedInResult.Concat(newCodesUsedInSource).ToArray();

			// Надо ли проверять наличие кода в погрузочниках или везде??

			if(newCodesUsed.Length > 0)
			{
				var usedIdentificationCodes = newCodesUsed
					.Select(x => x.ResultCode?.IdentificationCode
						?? x.SourceCode?.IdentificationCode)
					.ToArray();

				_logger.LogWarning(
					"Отсканированные коды уже использованы: {@IdentificationCodes}",
					usedIdentificationCodes);

				var error = Errors.TrueMark.CodesAlreadyInUse;

				var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);
				return RequestProcessingResult.CreateFailure(result, new TrueMarkCodeProcessingResultResponse
				{
					Nomenclature = null,
					Result = RequestProcessingResultTypeDto.Error,
					Error = error.Message + ": " + string.Join(", ", usedIdentificationCodes)
				});
			}

			var instanceCodes = trueMarkAnyCodes
				.Where(x => x.IsTrueMarkWaterIdentificationCode)
				.Select(x => x.TrueMarkWaterIdentificationCode);

			var codeCheckingResult = await _routeListItemTrueMarkProductCodesProcessingService.IsTrueMarkCodeCanBeAddedToRouteListItem(
				uow,
				instanceCodes,
				routeListAddress,
				vodovozOrderItem,
				cancellationToken,
				isCheckForCodeChange
			);

			if(codeCheckingResult.IsFailure)
			{
				uow.Session?.GetCurrentTransaction()?.Rollback();

				var error = codeCheckingResult.Errors.FirstOrDefault();

				var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);

				return RequestProcessingResult.CreateFailure(result, new TrueMarkCodeProcessingResultResponse
				{
					Nomenclature = null,
					Result = RequestProcessingResultTypeDto.Error,
					Error = error.Message
				});
			}

			NomenclatureTrueMarkCodesDto nomenclatureDto = null;

			var trueMarkCodes = new List<TrueMarkCodeDto>();

			foreach(var trueMarkAnyCode in trueMarkAnyCodes)
			{
				trueMarkCodes.Add(trueMarkAnyCode.Match(
					PopulateTransportCode(trueMarkAnyCodes),
					PopulateGroupCode(trueMarkAnyCodes),
					PopulateWaterCode(trueMarkAnyCodes)));

				if(!trueMarkAnyCode.IsTrueMarkWaterIdentificationCode)
				{
					continue;
				}

				_routeListItemTrueMarkProductCodesProcessingService
					.AddTrueMarkCodeToRouteListItem(
						uow,
						routeListAddress,
						vodovozOrderItem.Id,
						trueMarkAnyCode.TrueMarkWaterIdentificationCode,
						status,
						ProductCodeProblem.None);

				if(nomenclatureDto is null)
				{
					nomenclatureDto = _orderConverter.ConvertOrderItemTrueMarkCodesDataToDto(vodovozOrderItem, routeListAddress);
				}

				uow.Save(routeListAddress);
			}

			if(nomenclatureDto != null)
			{
				nomenclatureDto.Codes = trueMarkCodes;
			}

			var successResponse = new TrueMarkCodeProcessingResultResponse
			{
				Nomenclature = nomenclatureDto,
				Result = RequestProcessingResultTypeDto.Success,
				Error = null
			};

			if(nomenclatureDto != null)
			{
				nomenclatureDto.Codes = trueMarkCodes;
			}

			return RequestProcessingResult.CreateSuccess(Result.Success(successResponse));
		}

		public async Task<RequestProcessingResult<TrueMarkCodeProcessingResultResponse>> ChangeTrueMarkCodeToRouteListItemWithCodeChecking(
			RouteListItem routeListAddress,
			OrderItem vodovozOrderItem,
			string oldScannedCode,
			string newScannedCode,
			SourceProductCodeStatus status,
			CancellationToken cancellationToken)
		{
			var oldTrueMarkCodeResult = await _trueMarkWaterCodeService.GetTrueMarkCodeByScannedCode(_uow, oldScannedCode, cancellationToken);

			if(oldTrueMarkCodeResult.IsFailure)
			{
				var error = oldTrueMarkCodeResult.Errors.FirstOrDefault();
				var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);
				return RequestProcessingResult.CreateFailure(result, new TrueMarkCodeProcessingResultResponse
				{
					Nomenclature = null,
					Result = RequestProcessingResultTypeDto.Error,
					Error = error.Message
				});
			}

			Result<TrueMarkAnyCode> newTrueMarkCodeResult = null;

			if(!string.IsNullOrWhiteSpace(newScannedCode))
			{
				newTrueMarkCodeResult = await _trueMarkWaterCodeService.GetTrueMarkCodeByScannedCode(_uow, newScannedCode);

				if(newTrueMarkCodeResult.IsFailure)
				{
					var error = newTrueMarkCodeResult.Errors.FirstOrDefault();
					var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);
					return RequestProcessingResult.CreateFailure(result, new TrueMarkCodeProcessingResultResponse
					{
						Nomenclature = null,
						Result = RequestProcessingResultTypeDto.Error,
						Error = error.Message
					});
				}
			}

			if(oldTrueMarkCodeResult.Value.Match(
				transportCode => transportCode.ParentTransportCodeId != null,
				groupCode => groupCode.ParentTransportCodeId != null
					|| groupCode.ParentWaterGroupCodeId != null,
				waterCode => waterCode.ParentTransportCodeId != null
					|| waterCode.ParentWaterGroupCodeId != null))
			{
				var error = Errors.TrueMark.AggregatedCodeChangeAttempt;
				var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);
				return RequestProcessingResult.CreateFailure(result, new TrueMarkCodeProcessingResultResponse
				{
					Nomenclature = null,
					Result = RequestProcessingResultTypeDto.Error,
					Error = error.Message
				});
			}

			if(newTrueMarkCodeResult != null
				&& newTrueMarkCodeResult.Value.Match(
					transportCode => transportCode.ParentTransportCodeId != null,
					groupCode => groupCode.ParentTransportCodeId != null
						|| groupCode.ParentWaterGroupCodeId != null,
					waterCode => waterCode.ParentTransportCodeId != null
						|| waterCode.ParentWaterGroupCodeId != null))
			{
				var error = Errors.TrueMark.ToAggregatedCodeChangeAttempt;
				var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);
				return RequestProcessingResult.CreateFailure(result, new TrueMarkCodeProcessingResultResponse
				{
					Nomenclature = null,
					Result = RequestProcessingResultTypeDto.Error,
					Error = error.Message
				});
			}

			IEnumerable<TrueMarkAnyCode> oldTrueMarkAnyCodes = oldTrueMarkCodeResult.Value.Match(
				transportCode => transportCode.GetAllCodes(),
				groupCode => groupCode.GetAllCodes(),
				waterCode => new TrueMarkAnyCode[] { waterCode })
				.ToArray();

			IEnumerable<TrueMarkAnyCode> newTrueMarkAnyCodes = (newTrueMarkCodeResult?.Value.Match(
				transportCode => transportCode.GetAllCodes(),
				groupCode => groupCode.GetAllCodes(),
				waterCode => new TrueMarkAnyCode[] { waterCode }) ?? Enumerable.Empty<TrueMarkAnyCode>())
				.ToArray();

			var newIdentificationCodes = newTrueMarkAnyCodes
				.Where(x => x.IsTrueMarkWaterIdentificationCode)
				.Select(x => x.TrueMarkWaterIdentificationCode)
				.ToArray();

			var newIdentificationCodesIds = newIdentificationCodes.Select(x => x.Id).ToArray();

			var newCodesUsedInResult = _uow.Session.Query<CarLoadDocumentItemTrueMarkProductCode>()
				.Where(x => newIdentificationCodesIds
					.Contains(x.ResultCode.Id))
				.ToArray();

			var newCodesUsedInSource = _uow.Session.Query<CarLoadDocumentItemTrueMarkProductCode>()
				.Where(x => newIdentificationCodesIds
					.Contains(x.SourceCode.Id))
				.ToArray();

			var newCodesUsed = newCodesUsedInResult.Concat(newCodesUsedInSource).ToArray();

			// Надо ли проверять наличие кода в погрузочниках или везде??

			if(newCodesUsed.Length > 0)
			{
				var usedIdentificationCodes = newCodesUsed
									.Select(x => x.ResultCode?.IdentificationCode
										?? x.SourceCode?.IdentificationCode)
									.ToArray();

				_logger.LogWarning(
					"Отсканированные коды уже использованы: {@IdentificationCodes}",
					usedIdentificationCodes);

				var error = Errors.TrueMark.CodesAlreadyInUse;

				var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);
				return RequestProcessingResult.CreateFailure(result, new TrueMarkCodeProcessingResultResponse
				{
					Nomenclature = null,
					Result = RequestProcessingResultTypeDto.Error,
					Error = error.Message + ": " + string.Join(", ", usedIdentificationCodes)
				});
			}

			foreach(var codeToRemove in oldTrueMarkAnyCodes)
			{
				if(!codeToRemove.IsTrueMarkWaterIdentificationCode)
				{
					continue;
				}

				var result = RemoveTrueMarkCodeFromRouteListItem(
					_uow,
					routeListAddress,
					vodovozOrderItem.Id,
					codeToRemove.TrueMarkWaterIdentificationCode);

				if(result.IsFailure)
				{
					_uow.Session.GetCurrentTransaction().Rollback();

					return RequestProcessingResult.CreateFailure(
						Result.Failure<TrueMarkCodeProcessingResultResponse>(result.Errors),
						new TrueMarkCodeProcessingResultResponse
						{
							Nomenclature = null,
							Result = RequestProcessingResultTypeDto.Error,
							Error = $"Не удалось удалить код: {string.Join(", ", result.Errors.Select(x => x.Message))}",
						});
				}
			}

			foreach(var oldCodeToRemoveFromDatabase in oldTrueMarkAnyCodes)
			{
				oldCodeToRemoveFromDatabase.Match(
					transportCode =>
					{
						transportCode.InnerTransportCodes.Clear();
						transportCode.InnerGroupCodes.Clear();
						_uow.Delete(transportCode);
						return true;
					},
					groupCode =>
					{
						groupCode.InnerGroupCodes.Clear();
						_uow.Delete(groupCode);
						return true;
					},
					waterCode =>
					{
						_uow.Delete(waterCode);
						return true;
					});
			}

			NomenclatureTrueMarkCodesDto nomenclatureDto = null;

			var trueMarkCodes = new List<TrueMarkCodeDto>();

			var addCodesResult = await AddTrueMarkCodeToRouteListItemWithCodeChecking(
				_uow,
				routeListAddress,
				vodovozOrderItem,
				newScannedCode,
				status,
				cancellationToken,
				true);
			
			_uow.Save(routeListAddress);

			try
			{
				_uow.Commit();
			}
			catch(Exception e)
			{
				_uow.Session.GetCurrentTransaction()?.Rollback();
				_logger.LogError(e, "Exception while commiting: {ExceptionMessage}", e.Message);
				throw;
			}

			return addCodesResult;
		}

		public Result RemoveTrueMarkCodeFromRouteListItem(
			IUnitOfWork uow,
			RouteListItem routeListAddress,
			int vodovozOrderItemId,
			TrueMarkWaterIdentificationCode codeToRemove)
		{
			var productCode =
				routeListAddress.TrueMarkCodes
					.Where(x => x.SourceCode.Id == codeToRemove.Id)
					.FirstOrDefault();

			if(productCode is null)
			{
				var error = TrueMarkCodeErrors.TrueMarkCodeForRouteListItemNotFound;
				return Result.Failure(error);
			}

			routeListAddress.TrueMarkCodes.Remove(productCode);

			var productCodeOrderItem = _orderRepository.GetTrueMarkCodesAddedByDriverToOrderItemByOrderItemId(uow, vodovozOrderItemId)
				.Where(x => x.TrueMarkProductCodeId == productCode.Id)
				.FirstOrDefault();

			if(productCodeOrderItem != null)
			{
				uow.Delete(productCodeOrderItem);
			}

			uow.Save(routeListAddress);

			return Result.Success();
		}

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
					Level = DriverApiTruemarkCodeLevel.unit,
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
					Level = DriverApiTruemarkCodeLevel.group,
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
					Level = DriverApiTruemarkCodeLevel.transport,
					Parent = parentRawCode
				};
			};
		}

		public async Task<Result> SendTrueMarkCodes(
			DateTime actionTime,
			Employee driver,
			int orderId,
			IEnumerable<OrderItemScannedBottlesDto> scannedBottles,
			string unscannedBottlesReason,
			CancellationToken cancellationToken)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				return Result.Failure(OrderErrors.NotFound);
			}

			if(vodovozOrder.IsNeedIndividualSetOnLoad(_edoAccountController)
			   || vodovozOrder.Client.ReasonForLeaving != ReasonForLeaving.ForOwnNeeds)
			{
				_logger.LogWarning("Заказ {OrderId} не является заказом для собственных нужд", orderId);
				return Result.Failure(OrderErrors.OrderIsNotForPersonalUseError);
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(RouteListErrors.NotFoundAssociatedWithOrder);
			}

			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(RouteListItemErrors.NotFoundAssociatedWithOrder);
			}

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning("Сотрудник {EmployeeId} попытался добавить коды к заказу {OrderId} водителя {DriverId}",
					driver.Id, orderId, routeList.Driver.Id);
				return Result.Failure(Errors.Security.Authorization.OrderAccessDenied);
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя добавить коды к заказу: {OrderId}, МЛ не в пути", orderId);
				return Result.Failure<PayByQrResponse>(RouteListErrors.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя добавить коды к заказу: {OrderId}, адрес МЛ {RouteListAddressId} не в пути", orderId, routeListAddress.Id);
				return Result.Failure<PayByQrResponse>(RouteListItemErrors.NotEnRouteState);
			}

			foreach(var scannedBottle in scannedBottles)
			{
				var orderSaleItemId = scannedBottle.OrderSaleItemId;

				if(!vodovozOrder.OrderItems.Any(x => x.Id == orderSaleItemId))
				{
					_logger.LogWarning("У заказа {OrderId} заказа не найдена: {OrderItemId}", orderSaleItemId);
					return Result.Failure(OrderItemErrors.NotFound);
				}
				
				var bottleCodes = scannedBottle.BottleCodes
					.Distinct()
					.Select(x => new DriversScannedTrueMarkCode
					{
						RawCode = x,
						OrderItemId = orderSaleItemId,
						RouteListAddressId = routeListAddress.Id,
						IsDefective = false
					})
					.ToArray();

				foreach(var scannedCode in bottleCodes)
				{
					await _uow.SaveAsync(scannedCode, cancellationToken: cancellationToken);
				}
			}

			await _uow.CommitAsync(cancellationToken);

			return Result.Success();
		}
	}
}
