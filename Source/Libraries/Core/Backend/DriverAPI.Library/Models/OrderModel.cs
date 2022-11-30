using DriverAPI.Library.Converters;
using DriverAPI.Library.DTOs;
using DriverAPI.Library.Helpers;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;

namespace DriverAPI.Library.Models
{
	public class OrderModel : IOrderModel
	{
		private readonly ILogger<OrderModel> _logger;
		private readonly IOrderRepository _orderRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly OrderConverter _orderConverter;
		private readonly IDriverApiParametersProvider _webApiParametersProvider;
		private readonly IComplaintsRepository _complaintsRepository;
		private readonly ISmsPaymentModel _aPISmsPaymentModel;
		private readonly ISmsPaymentServiceAPIHelper _smsPaymentServiceAPIHelper;
		private readonly IFastPaymentsServiceAPIHelper _fastPaymentsServiceApiHelper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly QRPaymentConverter _qrPaymentConverter;
		private readonly IFastPaymentModel _fastPaymentModel;
		private readonly int _maxClosingRating = 5;
		private readonly PaymentType[] _smsAndQRNotPayable = new PaymentType[] { PaymentType.ByCard, PaymentType.barter, PaymentType.ContractDoc };

		public OrderModel(
			ILogger<OrderModel> logger,
			IOrderRepository orderRepository,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			OrderConverter orderConverter,
			IDriverApiParametersProvider webApiParametersProvider,
			IComplaintsRepository complaintsRepository,
			ISmsPaymentModel aPISmsPaymentModel,
			ISmsPaymentServiceAPIHelper smsPaymentServiceAPIHelper,
			IFastPaymentsServiceAPIHelper fastPaymentsServiceApiHelper,
			IUnitOfWork unitOfWork,
			QRPaymentConverter qrPaymentConverter,
			IFastPaymentModel fastPaymentModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_orderConverter = orderConverter ?? throw new ArgumentNullException(nameof(orderConverter));
			_webApiParametersProvider = webApiParametersProvider ?? throw new ArgumentNullException(nameof(webApiParametersProvider));
			_complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			_aPISmsPaymentModel = aPISmsPaymentModel ?? throw new ArgumentNullException(nameof(aPISmsPaymentModel));
			_smsPaymentServiceAPIHelper = smsPaymentServiceAPIHelper ?? throw new ArgumentNullException(nameof(smsPaymentServiceAPIHelper));
			_fastPaymentsServiceApiHelper = fastPaymentsServiceApiHelper ?? throw new ArgumentNullException(nameof(fastPaymentsServiceApiHelper));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_qrPaymentConverter = qrPaymentConverter ?? throw new ArgumentNullException(nameof(qrPaymentConverter));
			_fastPaymentModel = fastPaymentModel ?? throw new ArgumentNullException(nameof(fastPaymentModel));
		}

		/// <summary>
		/// Получение заказа в требуемом формате из заказа программы ДВ (использует функцию ниже)
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>APIOrder</returns>
		public OrderDto Get(int orderId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_unitOfWork, orderId)
				?? throw new DataNotFoundException(nameof(orderId), $"Заказ { orderId } не найден");
			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(_unitOfWork, vodovozOrder);

			var order = _orderConverter.convertToAPIOrder(
				vodovozOrder,
				routeListItem.CreationDate,
				_aPISmsPaymentModel.GetOrderSmsPaymentStatus(orderId),
				_fastPaymentModel.GetOrderFastPaymentStatus(orderId));
			order.OrderAdditionalInfo = GetAdditionalInfo(vodovozOrder);

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
			var vodovozOrders = _orderRepository.GetOrders(_unitOfWork, orderIds);

			foreach(var vodovozOrder in vodovozOrders)
			{
				var smsPaymentStatus = _aPISmsPaymentModel.GetOrderSmsPaymentStatus(vodovozOrder.Id);
				var qrPaymentStatus = _fastPaymentModel.GetOrderFastPaymentStatus(vodovozOrder.Id);
				var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(_unitOfWork, vodovozOrder);
				var order = _orderConverter.convertToAPIOrder(vodovozOrder, routeListItem.CreationDate, smsPaymentStatus, qrPaymentStatus);
				order.OrderAdditionalInfo = GetAdditionalInfo(vodovozOrder);
				result.Add(order);
			}

			return result;
		}

		/// <summary>
		/// Получение типов оплаты на которые можно изменить тип оплаты заказа переданного в аргументе
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>IEnumerable APIPaymentType</returns>
		public IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(int orderId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_unitOfWork, orderId)
				?? throw new DataNotFoundException(nameof(orderId), $"Заказ { orderId } не найден");

			return GetAvailableToChangePaymentTypes(vodovozOrder);
		}

		/// <summary>
		/// Получение типов оплаты на которые можно изменить тип оплаты заказа переданного в аргументе
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <returns>IEnumerable APIPaymentType</returns>
		public IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(Order order)
		{
			var availablePaymentTypes = new List<PaymentDtoType>();

			if(order.PaymentType == PaymentType.cash)
			{
				availablePaymentTypes.Add(PaymentDtoType.Terminal);
			}

			if(order.PaymentType == PaymentType.Terminal)
			{
				availablePaymentTypes.Add(PaymentDtoType.Cash);
			}

			return availablePaymentTypes;
		}

		/// <summary>
		/// Получение дополнительной информации для заказа по идентификатору
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>APIOrderAdditionalInfo</returns>
		public OrderAdditionalInfoDto GetAdditionalInfo(int orderId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_unitOfWork, orderId)
				?? throw new DataNotFoundException(nameof(orderId), $"Заказ { orderId } не найден");

			return GetAdditionalInfo(vodovozOrder);
		}

		/// <summary>
		/// Получение дополнительной информации для заказа из заказа программы ДВ
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <returns>APIOrderAdditionalInfo</returns>
		public OrderAdditionalInfoDto GetAdditionalInfo(Order order)
		{
			return new OrderAdditionalInfoDto
			{
				AvailablePaymentTypes = GetAvailableToChangePaymentTypes(order),
				CanSendSms = CanSendSmsForPayment(order, _aPISmsPaymentModel.GetOrderSmsPaymentStatus(order.Id)),
				CanReceiveQRCode = CanReceiveQRCodeForPayment(order),
			};
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

		public void ChangeOrderPaymentType(int orderId, PaymentType paymentType, Employee driver)
		{
			if(driver is null)
			{
				throw new ArgumentNullException(nameof(driver));
			}

			var vodovozOrder = _orderRepository.GetOrder(_unitOfWork, orderId)
				?? throw new DataNotFoundException(nameof(orderId), $"Заказ { orderId } не найден");

			if(vodovozOrder.OrderStatus != OrderStatus.OnTheWay)
			{
				throw new InvalidOperationException($"Нельзя изменить тип оплаты для заказа: { orderId }, заказ не в пути.");
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_unitOfWork, vodovozOrder);

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning($"Водитель {driver.Id} попытался сменить тип оплаты заказа {orderId} водителя {routeList.Driver.Id}");
				throw new InvalidOperationException("Нельзя сменить тип оплаты заказа другого водителя");
			}

			vodovozOrder.PaymentType = paymentType;
			_unitOfWork.Save(vodovozOrder);
			_unitOfWork.Commit();
		}

		public void CompleteOrderDelivery(
			Employee driver,
			int orderId,
			int bottlesReturnCount,
			int rating,
			int driverComplaintReasonId,
			string otherDriverComplaintReasonComment,
			string driverComment,
			DateTime actionTime)
		{
			var vodovozOrder = _orderRepository.GetOrder(_unitOfWork, orderId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_unitOfWork, vodovozOrder);
			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(vodovozOrder is null)
			{
				var error = $"Заказ не найден: { orderId }";
				_logger.LogWarning(error);
				throw new ArgumentOutOfRangeException(nameof(orderId), error);
			}

			if(routeList is null)
			{
				var error = $"МЛ для заказа: { orderId } не найден";
				_logger.LogWarning(error);
				throw new ArgumentOutOfRangeException(nameof(orderId), error);
			}

			if(routeListAddress is null)
			{
				var error = $"адрес МЛ для заказа: { orderId } не найден";
				_logger.LogWarning(error);
				throw new ArgumentOutOfRangeException(nameof(orderId), error);
			}

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning("Водитель {DriverId} попытался завершить заказ {OrderId} водителя {RouteListDriverId}",
					driver.Id, orderId, routeList.Driver.Id);
				throw new InvalidOperationException("Нельзя завершить заказ другого водителя");
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				var error = $"Нельзя завершить заказ: { orderId }, МЛ не в пути";
				_logger.LogWarning(error);
				throw new ArgumentOutOfRangeException(nameof(orderId), error);
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				var error = $"Нельзя завершить заказ: { orderId }, адрес МЛ не в пути";
				_logger.LogWarning(error);
				throw new ArgumentOutOfRangeException(nameof(orderId), error);
			}

			routeListAddress.DriverBottlesReturned = bottlesReturnCount;

			routeList.ChangeAddressStatus(_unitOfWork, routeListAddress.Id, RouteListItemStatus.Completed);

			if (rating < _maxClosingRating)
			{
				var complaintReason = _complaintsRepository.GetDriverComplaintReasonById(_unitOfWork, driverComplaintReasonId);
				var complaintSource = _complaintsRepository.GetComplaintSourceById(_unitOfWork, _webApiParametersProvider.ComplaintSourceId);
				var reason = complaintReason?.Name ?? otherDriverComplaintReasonComment;

				var complaint = new Complaint
				{
					ComplaintSource = complaintSource,
					ComplaintType = ComplaintType.Driver,
					Order = vodovozOrder,
					DriverRating = rating,
					DeliveryPoint = vodovozOrder.DeliveryPoint,
					CreationDate = actionTime,
					ChangedDate = actionTime,
					CreatedBy = driver,
					ChangedBy = driver,
					ComplaintText = $"Заказ номер { orderId }\n" +
						$"По причине { reason }"
				};

				_unitOfWork.Save(complaint);
			}

			if(bottlesReturnCount != vodovozOrder.BottlesReturn)
			{
				if(!string.IsNullOrWhiteSpace(driverComment))
				{
					vodovozOrder.DriverMobileAppComment = driverComment;
					vodovozOrder.DriverMobileAppCommentTime = actionTime;
				}

				vodovozOrder.DriverCallType = DriverCallType.CommentFromMobileApp;

				_unitOfWork.Save(vodovozOrder);
			}

			_unitOfWork.Save(routeListAddress);
			_unitOfWork.Save(routeList);

			_unitOfWork.Commit();
		}

		public void SendSmsPaymentRequest(
			int orderId,
			string phoneNumber,
			int driverId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_unitOfWork, orderId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_unitOfWork, vodovozOrder);
			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(vodovozOrder is null
			|| routeList is null
			|| routeListAddress is null)
			{
				throw new DataNotFoundException(nameof(orderId), "Не найден или не находится в МЛ");
			}

			if(routeList.Status != RouteListStatus.EnRoute
			|| routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				throw new InvalidOperationException("Нельзя отправлять СМС на оплату для адреса МЛ не в пути");
			}

			if(routeList.Driver.Id != driverId)
			{
				_logger.LogWarning("Водитель {DriverId} попытался запросить оплату по СМС для заказа {OrderId} водителя {RouteListDriverId}",
					driverId, orderId, routeList.Driver.Id);
				throw new InvalidOperationException("Нельзя запросить оплату по СМС для заказа другого водителя");
			}

			_smsPaymentServiceAPIHelper.SendPayment(orderId, phoneNumber).Wait();
		}
		
		public async Task<PayByQRResponseDTO> SendQRPaymentRequestAsync(int orderId, int driverId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_unitOfWork, orderId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_unitOfWork, vodovozOrder);
			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(vodovozOrder is null || routeList is null || routeListAddress is null)
			{
				throw new DataNotFoundException(nameof(orderId), "Не найден или не находится в МЛ");
			}

			if(routeList.Status != RouteListStatus.EnRoute || routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				throw new InvalidOperationException("Нельзя отправлять QR-код на оплату для адреса МЛ не в пути");
			}

			if(routeList.Driver.Id != driverId)
			{
				_logger.LogWarning("Водитель {DriverId} попытался запросить оплату по QR для заказа {OrderId} водителя {RouteListDriverId}",
					driverId, orderId, routeList.Driver.Id);
				throw new InvalidOperationException("Нельзя запросить оплату по QR для заказа другого водителя");
			}

			var qrResponseDto = await _fastPaymentsServiceApiHelper.SendPaymentAsync(orderId);
			var payByQRResponseDto = _qrPaymentConverter.ConvertToPayByQRResponseDto(qrResponseDto);

			if(payByQRResponseDto.QRPaymentStatus == QRPaymentDTOStatus.Paid)
			{
				payByQRResponseDto.AvailablePaymentTypes = Enumerable.Empty<PaymentDtoType>();
				payByQRResponseDto.CanReceiveQR = false;
			}
			else
			{
				payByQRResponseDto.AvailablePaymentTypes = GetAvailableToChangePaymentTypes(orderId);
				payByQRResponseDto.CanReceiveQR = true;
			}

			return payByQRResponseDto;
		}
	}
}
