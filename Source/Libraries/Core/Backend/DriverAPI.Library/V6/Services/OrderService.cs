using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Responses;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.V6.Converters;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Errors;
using Vodovoz.Extensions;
using Vodovoz.Models.TrueMark;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Services.TrueMark;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCode;

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
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly QrPaymentConverter _qrPaymentConverter;
		private readonly IFastPaymentService _fastPaymentModel;
		private readonly int _maxClosingRating = 5;
		private readonly PaymentType[] _smsAndQRNotPayable = new PaymentType[] { PaymentType.PaidOnline, PaymentType.Barter, PaymentType.ContractDocumentation };
		private readonly IOrderSettings _orderSettings;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;

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
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			QrPaymentConverter qrPaymentConverter,
			IFastPaymentService fastPaymentModel,
			IOrderSettings orderSettings,
			ITrueMarkWaterCodeService trueMarkWaterCodeService)
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
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_qrPaymentConverter = qrPaymentConverter ?? throw new ArgumentNullException(nameof(qrPaymentConverter));
			_fastPaymentModel = fastPaymentModel ?? throw new ArgumentNullException(nameof(fastPaymentModel));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
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
				return Result.Failure<OrderDto>(Vodovoz.Errors.Orders.Order.NotFound);
			}

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(_uow, vodovozOrder);

			if(routeListItem is null)
			{
				return Result.Failure<OrderDto>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFoundAssociatedWithOrder);
			}

			var order = _orderConverter.ConvertToAPIOrder(
				vodovozOrder,
				routeListItem,
				_aPISmsPaymentModel.GetOrderSmsPaymentStatus(orderId),
				_fastPaymentModel.GetOrderFastPaymentStatus(orderId, vodovozOrder.OnlineOrder));

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
			var vodovozOrders = _orderRepository.GetOrders(_uow, orderIds);

			foreach(var vodovozOrder in vodovozOrders)
			{
				var smsPaymentStatus = _aPISmsPaymentModel.GetOrderSmsPaymentStatus(vodovozOrder.Id);
				var qrPaymentStatus = _fastPaymentModel.GetOrderFastPaymentStatus(vodovozOrder.Id, vodovozOrder.OnlineOrder);
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
				return Result.Failure<IEnumerable<PaymentDtoType>>(Vodovoz.Errors.Orders.Order.NotFound);
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
				return Result.Failure<OrderAdditionalInfoDto>(Vodovoz.Errors.Orders.Order.NotFound);
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

				return Result.Failure(Vodovoz.Errors.Orders.Order.NotFound);
			}

			if(vodovozOrder.OrderStatus != OrderStatus.OnTheWay)
			{
				_logger.LogWarning("Нельзя изменить тип оплаты для заказа: {OrderId}, заказ не в статусе {OrderStatus}.",
					orderId,
					OrderStatus.OnTheWay.GetEnumDisplayName());

				return Result.Failure(Vodovoz.Errors.Orders.Order.NotInOnTheWayStatus);
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);

				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.NotFoundAssociatedWithOrder);
			}

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning("Сотрудник {EmployeeId} попытался сменить тип оплаты заказа {OrderId} водителя {DriverId}",
					driver.Id,
					orderId,
					routeList.Driver.Id);

				return Result.Failure(Errors.Security.Authorization.OrderAccessDenied);
			}

			vodovozOrder.PaymentType = paymentType;
			vodovozOrder.PaymentByTerminalSource = paymentByTerminalSource;

			_uow.Save(vodovozOrder);
			_uow.Commit();

			return Result.Success();
		}

		public Result CompleteOrderDelivery(DateTime actionTime, Employee driver, IDriverOrderShipmentInfo completeOrderInfo, IDriverComplaintInfo driverComplaintInfo)
		{
			var orderId = completeOrderInfo.OrderId;
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);
			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				return Result.Failure(Vodovoz.Errors.Orders.Order.NotFound);
			}

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.NotFoundAssociatedWithOrder);
			}

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFoundAssociatedWithOrder);
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
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя завершить заказ: {OrderId}, адрес МЛ {RouteListAddressId} не в пути", orderId, routeListAddress.Id);
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotEnRouteState);
			}

			var trueMarkCodesProcessResult =
				ProcessScannedCodes(actionTime, completeOrderInfo, routeListAddress);

			if(trueMarkCodesProcessResult.IsFailure)
			{
				return trueMarkCodesProcessResult;
			}

			routeListAddress.DriverBottlesReturned = completeOrderInfo.BottlesReturnCount;

			routeList.ChangeAddressStatus(_uow, routeListAddress.Id, RouteListItemStatus.Completed);

			CreateComplaintIfNeeded(driverComplaintInfo, vodovozOrder, driver, actionTime);

			if(completeOrderInfo.BottlesReturnCount != vodovozOrder.BottlesReturn)
			{
				if(!string.IsNullOrWhiteSpace(completeOrderInfo.DriverComment))
				{
					vodovozOrder.DriverMobileAppComment = completeOrderInfo.DriverComment;
					vodovozOrder.DriverMobileAppCommentTime = actionTime;
				}

				vodovozOrder.DriverCallType = DriverCallType.CommentFromMobileApp;

				_uow.Save(vodovozOrder);
			}

			_uow.Save(routeListAddress);
			_uow.Save(routeList);

			_uow.Commit();

			return Result.Success();
		}

		public Result UpdateOrderShipmentInfo(
			DateTime actionTime,
			Employee driver,
			IDriverOrderShipmentInfo completeOrderInfo)
		{
			var orderId = completeOrderInfo.OrderId;
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);
			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				return Result.Failure(Vodovoz.Errors.Orders.Order.NotFound);
			}

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.NotFoundAssociatedWithOrder);
			}

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFoundAssociatedWithOrder);
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
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя завершить заказ: {OrderId}, адрес МЛ {RouteListAddressId} не в пути", orderId, routeListAddress.Id);
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotEnRouteState);
			}

			var trueMarkCodesProcessResult =
				ProcessScannedCodes(actionTime, completeOrderInfo, routeListAddress);

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

				_uow.Save(vodovozOrder);
			}

			_uow.Save(routeListAddress);
			_uow.Save(routeList);

			_uow.Commit();

			return Result.Success();
		}

		public async Task<Result<PayByQrResponse>> SendQrPaymentRequestAsync(int orderId, int driverId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);
			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(vodovozOrder is null)
			{
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Orders.Order.NotFound);
			}

			if(routeList is null)
			{
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.NotFoundAssociatedWithOrder);
			}

			if(routeListAddress is null)
			{
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFoundAssociatedWithOrder);
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotEnRouteState);
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
				return Result.Failure(Vodovoz.Errors.Orders.Order.NotFound);
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

		private Result ProcessScannedCodes(DateTime actionTime, IDriverOrderShipmentInfo completeOrderInfo, RouteListItem routeListAddress)
		{
			if(routeListAddress.Order.Client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
			{
				return ProcessOwnUseOrderScannedCodes(actionTime, completeOrderInfo, routeListAddress);
			}

			return ProcessResaleOrderScannedCodes(completeOrderInfo, routeListAddress);
		}

		private Result ProcessOwnUseOrderScannedCodes(DateTime actionTime, IDriverOrderShipmentInfo completeOrderInfo, RouteListItem routeListAddress)
		{
			var scannedCodes = completeOrderInfo.ScannedItems.SelectMany(x => x.BottleCodes).ToList();

			routeListAddress.TrueMarkCodes.Clear();
			routeListAddress.UnscannedCodesReason = completeOrderInfo.UnscannedCodesReason;

			foreach(var scannedCode in scannedCodes)
			{
				var trueMarkWaterIdentificationCode =
					_trueMarkWaterCodeService.LoadOrCreateTrueMarkWaterIdentificationCode(_uow, scannedCode);

				if(routeListAddress.TrueMarkCodes.Any(x => x.SourceCode.RawCode == trueMarkWaterIdentificationCode.RawCode))
				{
					continue;
				}

				var productCode = new RouteListItemTrueMarkProductCode
				{
					CreationTime = actionTime,
					SourceCodeStatus = SourceProductCodeStatus.New,
					SourceCode = trueMarkWaterIdentificationCode,
					RouteListItem = routeListAddress
				};

				routeListAddress.TrueMarkCodes.Add(productCode);
			}

			return Result.Success();
		}

		private Result ProcessResaleOrderScannedCodes(IDriverOrderShipmentInfo completeOrderInfo, RouteListItem routeListAddress)
		{
			var processResult = IsScannedCodesAndSavedCodesAreEqual(completeOrderInfo, routeListAddress);

			if(processResult.IsFailure)
			{
				return processResult;
			}

			return IsAllRouteListItemTrueMarkProductCodesAddedToOrder(_uow, routeListAddress.Order.Id);
		}

		private Result IsScannedCodesAndSavedCodesAreEqual(IDriverOrderShipmentInfo completeOrderInfo, RouteListItem routeListAddress)
		{
			var scannedCodes = completeOrderInfo.ScannedItems.SelectMany(x => x.BottleCodes).ToList();
			var savedCodes = _orderRepository.GetAddedRouteListItemTrueMarkProductCodesByOrderId(_uow, routeListAddress.Order.Id);

			if(scannedCodes.Count != savedCodes.Count)
			{
				return Result.Failure(TrueMarkCodeErrors.AddedAndSavedCodesCountNotEquals);
			}

			foreach(var code in savedCodes)
			{
				if(!scannedCodes.Contains(code.SourceCode.RawCode))
				{
					return Result.Failure(TrueMarkCodeErrors.AddedAndSavedCodesNotEquals);
				}
			}

			return Result.Success();
		}

		private Result IsAllRouteListItemTrueMarkProductCodesAddedToOrder(IUnitOfWork uow, int orderId)
		{
			var isAllTrueMarkCodesAdded =
				_orderRepository.IsAllRouteListItemTrueMarkProductCodesAddedToOrder(_uow, orderId);

			if(!isAllTrueMarkCodesAdded)
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

		public async Task<Result> AddTrueMarkCode(
			DateTime actionTime,
			Employee driver,
			int orderId,
			int orderSaleItemId,
			string scannedCode,
			CancellationToken cancellationToken)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);
			var vodovozOrderItem = vodovozOrder.OrderItems.FirstOrDefault(x => x.Id == orderSaleItemId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);
			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				return Result.Failure(Vodovoz.Errors.Orders.Order.NotFound);
			}

			if(vodovozOrderItem is null)
			{
				_logger.LogWarning("Строка заказа не найдена: {OrderItemId}", orderSaleItemId);
				return Result.Failure(Vodovoz.Errors.Orders.OrderItem.NotFound);
			}

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.NotFoundAssociatedWithOrder);
			}

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFoundAssociatedWithOrder);
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
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя завершить заказ: {OrderId}, адрес МЛ {RouteListAddressId} не в пути", orderId, routeListAddress.Id);
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotEnRouteState);
			}

			var trueMarkWaterIdentificationCode =
				_trueMarkWaterCodeService.LoadOrCreateTrueMarkWaterIdentificationCode(_uow, scannedCode);

			var codeCheckingResult = await IsTrueMarkCodeCanBeAddedToRouteListItem(
				trueMarkWaterIdentificationCode,
				routeListAddress,
				vodovozOrder,
				vodovozOrderItem,
				cancellationToken);

			if(codeCheckingResult.IsFailure)
			{
				return codeCheckingResult;
			}

			var productCode = new RouteListItemTrueMarkProductCode
			{
				CreationTime = actionTime,
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				SourceCode = trueMarkWaterIdentificationCode,
				RouteListItem = routeListAddress
			};

			routeListAddress.TrueMarkCodes.Add(productCode);

			if(!cancellationToken.IsCancellationRequested)
			{
				_uow.Commit();
			}

			return Result.Success();
		}

		public async Task<Result> ChangeTrueMarkCode(
			DateTime actionTime,
			Employee driver,
			int orderId,
			int orderSaleItemId,
			string oldScannedCode,
			string newScannedCode,
			CancellationToken cancellationToken)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);
			var vodovozOrderItem = vodovozOrder.OrderItems.FirstOrDefault(x => x.Id == orderSaleItemId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);
			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				return Result.Failure(Vodovoz.Errors.Orders.Order.NotFound);
			}

			if(vodovozOrderItem is null)
			{
				_logger.LogWarning("Строка заказа не найдена: {OrderItemId}", orderSaleItemId);
				return Result.Failure(Vodovoz.Errors.Orders.OrderItem.NotFound);
			}

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.NotFoundAssociatedWithOrder);
			}

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFoundAssociatedWithOrder);
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
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя завершить заказ: {OrderId}, адрес МЛ {RouteListAddressId} не в пути", orderId, routeListAddress.Id);
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotEnRouteState);
			}

			var oldTrueMarkWaterIdentificationCode =
				_trueMarkWaterCodeService.LoadOrCreateTrueMarkWaterIdentificationCode(_uow, oldScannedCode);

			var newTrueMarkWaterIdentificationCode =
				_trueMarkWaterCodeService.LoadOrCreateTrueMarkWaterIdentificationCode(_uow, newScannedCode);

			if(oldTrueMarkWaterIdentificationCode.GTIN != newTrueMarkWaterIdentificationCode.GTIN)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodesGtinsNotEqual(oldScannedCode, newScannedCode);
				return Result.Failure(error);
			}

			var oldProductCode =
				routeListAddress.TrueMarkCodes
				.Where(x => x.SourceCode.Id == oldTrueMarkWaterIdentificationCode.Id)
				.FirstOrDefault();

			if(oldProductCode is null)
			{
				var error = TrueMarkCodeErrors.TrueMarkCodeForRouteListItemNotFound;
				return Result.Failure(error);
			}

			var codeCheckingResult = await IsTrueMarkCodeCanBeAddedToRouteListItem(
				newTrueMarkWaterIdentificationCode,
				routeListAddress,
				vodovozOrder,
				vodovozOrderItem,
				cancellationToken);

			if(codeCheckingResult.IsFailure)
			{
				return codeCheckingResult;
			}

			var productCode = new RouteListItemTrueMarkProductCode
			{
				CreationTime = actionTime,
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				SourceCode = newTrueMarkWaterIdentificationCode,
				ResultCode = newTrueMarkWaterIdentificationCode,
				RouteListItem = routeListAddress
			};

			routeListAddress.TrueMarkCodes.Remove(oldProductCode);
			routeListAddress.TrueMarkCodes.Add(productCode);

			if(!cancellationToken.IsCancellationRequested)
			{
				_uow.Commit();
			}

			return Result.Success();
		}

		public async Task<Result> RemoveTrueMarkCode(
			Employee driver,
			int orderId,
			int orderSaleItemId,
			string scannedCode,
			CancellationToken cancellationToken)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);
			var vodovozOrderItem = vodovozOrder.OrderItems.FirstOrDefault(x => x.Id == orderSaleItemId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);
			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				return Result.Failure(Vodovoz.Errors.Orders.Order.NotFound);
			}

			if(vodovozOrderItem is null)
			{
				_logger.LogWarning("Строка заказа не найдена: {OrderItemId}", orderSaleItemId);
				return Result.Failure(Vodovoz.Errors.Orders.OrderItem.NotFound);
			}

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.NotFoundAssociatedWithOrder);
			}

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ для заказа: {OrderId} не найден", orderId);
				return Result.Failure(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotFoundAssociatedWithOrder);
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
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.NotEnRouteState);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя завершить заказ: {OrderId}, адрес МЛ {RouteListAddressId} не в пути", orderId, routeListAddress.Id);
				return Result.Failure<PayByQrResponse>(Vodovoz.Errors.Logistics.RouteList.RouteListItem.NotEnRouteState);
			}

			var trueMarkWaterIdentificationCode =
				_trueMarkWaterCodeService.LoadOrCreateTrueMarkWaterIdentificationCode(_uow, scannedCode);

			var productCode =
				routeListAddress.TrueMarkCodes
				.Where(x => x.SourceCode.Id == trueMarkWaterIdentificationCode.Id)
				.FirstOrDefault();

			if(productCode is null)
			{
				var error = TrueMarkCodeErrors.TrueMarkCodeForRouteListItemNotFound;
				return Result.Failure(error);
			}

			routeListAddress.TrueMarkCodes.Remove(productCode);

			if(!cancellationToken.IsCancellationRequested)
			{
				_uow.Commit();
			}

			return Result.Success();
		}

		private async Task<Result> IsTrueMarkCodeCanBeAddedToRouteListItem(
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			RouteListItem routeListAddress,
			Order order,
			OrderItem orderItem,
			CancellationToken cancellationToken)
		{
			if(trueMarkWaterIdentificationCode.IsInvalid)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodeStringIsNotValid(trueMarkWaterIdentificationCode.RawCode);
				return Result.Failure(error);
			}

			if(trueMarkWaterIdentificationCode.GTIN != orderItem.Nomenclature.Gtin)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin(trueMarkWaterIdentificationCode.RawCode);
				return Result.Failure(error);
			}

			var addedCodesHavingRequiredGtin = routeListAddress.TrueMarkCodes
				.Where(x => x.SourceCode.GTIN == trueMarkWaterIdentificationCode.GTIN)
				.ToList();

			var bottlesInOrderHavingRequiredGtin = order.OrderItems
				.Where(x => x.Nomenclature.Gtin == trueMarkWaterIdentificationCode.GTIN)
				.Select(x => x.Count)
				.Sum();

			if(addedCodesHavingRequiredGtin.Count >= bottlesInOrderHavingRequiredGtin)
			{
				var error = TrueMarkCodeErrors.AllCodesAlreadyAdded;
				return Result.Failure(error);
			}

			var codeCheckingProcessResult =
				_trueMarkWaterCodeService.IsTrueMarkWaterIdentificationCodeNotUsed(trueMarkWaterIdentificationCode);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult =
				await _trueMarkWaterCodeService.IsTrueMarkCodeIntroducedAndHasCorrectInn(trueMarkWaterIdentificationCode, cancellationToken);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			return Result.Success();
		}
	}
}
