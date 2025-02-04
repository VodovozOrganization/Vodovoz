using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Responses;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.V6.Converters;
using Edo.Transport.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
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
using OrderErrors = Vodovoz.Errors.Orders.Order;
using OrderItemErrors = Vodovoz.Errors.Orders.OrderItem;
using RouteListErrors = Vodovoz.Errors.Logistics.RouteList;
using RouteListItemErrors = Vodovoz.Errors.Logistics.RouteList.RouteListItem;
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
		private readonly ITrueMarkWaterCodeCheckService _trueMarkWaterCodeCheckService;
		private readonly IRouteListItemTrueMarkProductCodesProcessingService _routeListItemTrueMarkProductCodesProcessingService;
		private readonly IBus _messageBus;

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
			ITrueMarkWaterCodeCheckService trueMarkWaterCodeCheckService,
			IRouteListItemTrueMarkProductCodesProcessingService routeListItemTrueMarkProductCodesProcessingService,
			IBus messageBus)
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
			_trueMarkWaterCodeCheckService = trueMarkWaterCodeCheckService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeCheckService));
			_routeListItemTrueMarkProductCodesProcessingService = routeListItemTrueMarkProductCodesProcessingService ?? throw new ArgumentNullException(nameof(routeListItemTrueMarkProductCodesProcessingService));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
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

			var productCodesByOrderItems =
				_orderRepository.GetTrueMarkCodesAddedByDriverToOrderByOrderId(_uow, vodovozOrder.Id);

			var order = _orderConverter.ConvertToAPIOrder(
				vodovozOrder,
				routeListItem,
				_aPISmsPaymentModel.GetOrderSmsPaymentStatus(orderId),
				_fastPaymentModel.GetOrderFastPaymentStatus(orderId, vodovozOrder.OnlineOrder),
				productCodesByOrderItems);

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
				var productCodesByOrderItems =
					_orderRepository.GetTrueMarkCodesAddedByDriverToOrderByOrderId(_uow, vodovozOrder.Id);

				var smsPaymentStatus = _aPISmsPaymentModel.GetOrderSmsPaymentStatus(vodovozOrder.Id);
				var qrPaymentStatus = _fastPaymentModel.GetOrderFastPaymentStatus(vodovozOrder.Id, vodovozOrder.OnlineOrder);
				var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(_uow, vodovozOrder);
				var order = _orderConverter.ConvertToAPIOrder(vodovozOrder, routeListItem, smsPaymentStatus, qrPaymentStatus, productCodesByOrderItems);
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

				return Result.Failure(Vodovoz.Errors.Orders.Order.NotFound);
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

		public async Task<Result> CompleteOrderDelivery(DateTime actionTime, Employee driver, IDriverOrderShipmentInfo completeOrderInfo, IDriverComplaintInfo driverComplaintInfo)
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

			var trueMarkCodesProcessResult =
				ProcessScannedCodes(completeOrderInfo, routeListAddress);

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

			OrderEdoRequest edoRequest = null;

			if(!vodovozOrder.IsOrderForResale || !vodovozOrder.IsOrderContainsIsAccountableInTrueMarkItems)
			{
				edoRequest = CreateEdoRequests(vodovozOrder, routeListAddress);
			}

			_uow.Commit();

			if(edoRequest != null)
			{
				await PublishEdoRequestCreatedEvent(edoRequest.Id);
			}

			return Result.Success();
		}

		private OrderEdoRequest CreateEdoRequests(Order vodovozOrder, RouteListItem routeListAddress)
		{
			var edoRequest = new OrderEdoRequest
			{
				Time = DateTime.Now,
				Source = CustomerEdoRequestSource.Warehouse,
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

		public Result UpdateOrderShipmentInfo(
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
				ProcessScannedCodes(completeOrderInfo, routeListAddress);

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

		private Result ProcessScannedCodes(
			IDriverOrderShipmentInfo completeOrderInfo,
			RouteListItem routeListAddress)
		{
			if(routeListAddress.Order.IsNeedIndividualSetOnLoad)
			{
				return ProcessNetworkClientOrderScannedCodes(routeListAddress);
			}

			if(routeListAddress.Order.Client.ReasonForLeaving == ReasonForLeaving.Resale)
			{
				return ProcessResaleOrderScannedCodes(routeListAddress);
			}

			return ProcessOwnUseOrderScannedCodes(completeOrderInfo, routeListAddress);
		}

		private Result ProcessOwnUseOrderScannedCodes(
			IDriverOrderShipmentInfo completeOrderInfo,
			RouteListItem routeListAddress)
		{
			var scannedCodes = completeOrderInfo.ScannedItems.SelectMany(x => x.BottleCodes).ToList();

			routeListAddress.UnscannedCodesReason = completeOrderInfo.UnscannedCodesReason;

			foreach(var scannedItem in completeOrderInfo.ScannedItems)
			{
				foreach(var scannedCode in scannedCodes)
				{
					var trueMarkWaterIdentificationCode =
						_trueMarkWaterCodeCheckService.LoadOrCreateTrueMarkWaterIdentificationCode(_uow, scannedCode);

					if(routeListAddress.TrueMarkCodes.Any(x => x.SourceCode.RawCode == trueMarkWaterIdentificationCode.RawCode))
					{
						continue;
					}

					_routeListItemTrueMarkProductCodesProcessingService.AddTrueMarkCodeToRouteListItem(
						_uow,
						routeListAddress,
						scannedItem.OrderSaleItemId,
						trueMarkWaterIdentificationCode,
						SourceProductCodeStatus.New);
				}
			}

			return Result.Success();
		}

		private Result ProcessResaleOrderScannedCodes(RouteListItem routeListAddress)
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

		private Result ProcessNetworkClientOrderScannedCodes(RouteListItem routeListAddress)
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
				var errorMessage = $"Заказ не найден: {orderId}";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(OrderErrors.NotFound, errorMessage: errorMessage);
			}

			var vodovozOrderItem = vodovozOrder.OrderItems.FirstOrDefault(x => x.Id == orderSaleItemId);

			if(vodovozOrderItem is null)
			{
				var errorMessage = $"Строка заказа не найдена: {orderSaleItemId}";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(OrderItemErrors.NotFound, errorMessage: errorMessage);
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList is null)
			{
				var errorMessage = $"МЛ для заказа: {orderId} не найден";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListErrors.NotFoundAssociatedWithOrder, errorMessage: errorMessage);
			}

			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(routeListAddress is null)
			{
				var errorMessage = $"Адрес МЛ для заказа: {orderId} не найден";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListItemErrors.NotFoundAssociatedWithOrder, errorMessage: errorMessage);
			}

			if(routeList.Driver.Id != driver.Id)
			{
				var errorMessage = $"Сотрудник {driver.Id} попытался завершить заказ {orderId} водителя {routeList.Driver.Id}";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(Errors.Security.Authorization.OrderAccessDenied, errorMessage: errorMessage);
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				var errorMessage = $"Нельзя завершить заказ: {orderId}, МЛ не в пути";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListErrors.NotEnRouteState, vodovozOrderItem, routeListAddress, errorMessage);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				var errorMessage = $"Нельзя завершить заказ: {orderId}, адрес МЛ {routeListAddress.Id} не в пути";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListItemErrors.NotEnRouteState, vodovozOrderItem, routeListAddress, errorMessage);
			}

			if(vodovozOrderItem.IsTrueMarkCodesMustBeAddedInWarehouse)
			{
				var errorMessage = $"Коды ЧЗ сетевого заказа {orderId} должны добавляться на складеде";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(TrueMarkCodeErrors.TrueMarkCodesHaveToBeAddedInWarehouse, vodovozOrderItem, routeListAddress, errorMessage);
			}
			
			var codeAddingResult = await _routeListItemTrueMarkProductCodesProcessingService.AddTrueMarkCodeToRouteListItemWithCodeChecking(
				_uow,
				routeListAddress,
				vodovozOrder,
				vodovozOrderItem,
				scannedCode,
				SourceProductCodeStatus.Accepted,
				cancellationToken);

			if(codeAddingResult.IsFailure)
			{
				var error = codeAddingResult.Errors.FirstOrDefault();
				return GetFailureTrueMarkCodeProcessingResponse(error, vodovozOrderItem, routeListAddress);
			}

			if(!cancellationToken.IsCancellationRequested)
			{
				_uow.Commit();
			}

			return GetSuccessTrueMarkCodeProcessingResponse(vodovozOrderItem, routeListAddress);
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
				var errorMessage = $"Заказ не найден: {orderId}";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(OrderErrors.NotFound, errorMessage: errorMessage);
			}

			var vodovozOrderItem = vodovozOrder.OrderItems.FirstOrDefault(x => x.Id == orderSaleItemId);

			if(vodovozOrderItem is null)
			{
				var errorMessage = $"Строка заказа не найдена: {orderSaleItemId}";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(OrderItemErrors.NotFound, errorMessage: errorMessage);
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList is null)
			{
				var errorMessage = $"МЛ для заказа: {orderId} не найден";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListErrors.NotFoundAssociatedWithOrder, errorMessage: errorMessage);
			}

			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(routeListAddress is null)
			{
				var errorMessage = $"Адрес МЛ для заказа: {orderId} не найден";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListItemErrors.NotFoundAssociatedWithOrder, errorMessage: errorMessage);
			}

			if(routeList.Driver.Id != driver.Id)
			{
				var errorMessage = $"Сотрудник {driver.Id} попытался завершить заказ {orderId} водителя {routeList.Driver.Id}";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(Errors.Security.Authorization.OrderAccessDenied, errorMessage: errorMessage);
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				var errorMessage = $"Нельзя завершить заказ: {orderId}, МЛ не в пути";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListErrors.NotEnRouteState, vodovozOrderItem, routeListAddress, errorMessage);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				var errorMessage = $"Нельзя завершить заказ: {orderId}, адрес МЛ {routeListAddress.Id} не в пути";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListItemErrors.NotEnRouteState, vodovozOrderItem, routeListAddress, errorMessage);
			}

			var changeCodeResult = await _routeListItemTrueMarkProductCodesProcessingService.ChangeTrueMarkCodeToRouteListItemWithCodeChecking(
				_uow,
				routeListAddress,
				vodovozOrder,
				vodovozOrderItem,
				oldScannedCode,
				newScannedCode,
				SourceProductCodeStatus.Accepted,
				cancellationToken);

			if(changeCodeResult.IsFailure)
			{
				var error = changeCodeResult.Errors.FirstOrDefault();
				return GetFailureTrueMarkCodeProcessingResponse(error, vodovozOrderItem, routeListAddress);
			}

			if(!cancellationToken.IsCancellationRequested)
			{
				_uow.Commit();
			}

			return GetSuccessTrueMarkCodeProcessingResponse(vodovozOrderItem, routeListAddress);
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
				var errorMessage = $"Заказ не найден: {orderId}";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(OrderErrors.NotFound, errorMessage: errorMessage);
			}

			var vodovozOrderItem = vodovozOrder.OrderItems.FirstOrDefault(x => x.Id == orderSaleItemId);

			if(vodovozOrderItem is null)
			{
				var errorMessage = $"Строка заказа не найдена: {orderSaleItemId}";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(OrderItemErrors.NotFound, errorMessage: errorMessage);
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList is null)
			{
				var errorMessage = $"МЛ для заказа: {orderId} не найден";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListErrors.NotFoundAssociatedWithOrder, errorMessage: errorMessage);
			}

			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(routeListAddress is null)
			{
				var errorMessage = $"Адрес МЛ для заказа: {orderId} не найден";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListItemErrors.NotFoundAssociatedWithOrder, errorMessage: errorMessage);
			}

			if(routeList.Driver.Id != driver.Id)
			{
				var errorMessage = $"Сотрудник {driver.Id} попытался завершить заказ {orderId} водителя {routeList.Driver.Id}";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(Errors.Security.Authorization.OrderAccessDenied, errorMessage: errorMessage);
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				var errorMessage = $"Нельзя завершить заказ: {orderId}, МЛ не в пути";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListErrors.NotEnRouteState, vodovozOrderItem, routeListAddress, errorMessage);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				var errorMessage = $"Нельзя завершить заказ: {orderId}, адрес МЛ {routeListAddress.Id} не в пути";
				_logger.LogWarning(errorMessage);
				return GetFailureTrueMarkCodeProcessingResponse(RouteListItemErrors.NotEnRouteState, vodovozOrderItem, routeListAddress, errorMessage);
			}

			var codeRemovingResult =
				_routeListItemTrueMarkProductCodesProcessingService.RemoveTrueMarkCodeFromRouteListItem(_uow, routeListAddress, vodovozOrderItem.Id, scannedCode);

			if(codeRemovingResult.IsFailure)
			{
				var error = codeRemovingResult.Errors.FirstOrDefault();
				return GetFailureTrueMarkCodeProcessingResponse(error, vodovozOrderItem, routeListAddress);
			}

			if(!cancellationToken.IsCancellationRequested)
			{
				_uow.Commit();
			}

			return GetSuccessTrueMarkCodeProcessingResponse(vodovozOrderItem, routeListAddress);
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
				var productCodesByOrderItems =
					_orderRepository.GetTrueMarkCodesAddedByDriverToOrderItemByOrderItemId(_uow, orderItem.Id);

				response.Nomenclature = _orderConverter.ConvertOrderItemTrueMarkCodesDataToDto(orderItem, routeListAddress, productCodesByOrderItems);
			}

			var result = Result.Failure<TrueMarkCodeProcessingResultResponse>(error);

			return RequestProcessingResult.CreateFailure(result, response);
		}

		private RequestProcessingResult<TrueMarkCodeProcessingResultResponse> GetSuccessTrueMarkCodeProcessingResponse(
			OrderItem orderItem,
			RouteListItem routeListAddress)
		{
			var response = new TrueMarkCodeProcessingResultResponse
			{
				Result = RequestProcessingResultTypeDto.Success
			};

			var productCodesByOrderItems =
				_orderRepository.GetTrueMarkCodesAddedByDriverToOrderItemByOrderItemId(_uow, orderItem.Id);

			if(orderItem != null && routeListAddress != null)
			{
				response.Nomenclature = _orderConverter.ConvertOrderItemTrueMarkCodesDataToDto(orderItem, routeListAddress, productCodesByOrderItems);
			}

			return RequestProcessingResult.CreateSuccess(Result.Success(response));
		}
	}
}
