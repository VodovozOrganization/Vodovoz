using DriverAPI.Library.Converters;
using DriverAPI.Library.DTOs;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
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
		private readonly OrderConverter _orderConverter;
		private readonly IOrderParametersProvider _orderParametersProvider;
		private readonly IDriverApiParametersProvider _webApiParametersProvider;
		private readonly IComplaintsRepository _complaintsRepository;
		private readonly ISmsPaymentModel _aPISmsPaymentData;
		private readonly IDriverMobileAppActionRecordModel _driverMobileAppActionRecordData;
		private readonly IUnitOfWork _unitOfWork;

		private readonly int _maxClosingRating = 5;

		public OrderModel(ILogger<OrderModel> logger,
			IOrderRepository orderRepository,
			IRouteListRepository routeListRepository,
			OrderConverter orderConverter,
			IOrderParametersProvider orderParametersProvider,
			IDriverApiParametersProvider webApiParametersProvider,
			IComplaintsRepository complaintsRepository,
			ISmsPaymentModel aPISmsPaymentData,
			IDriverMobileAppActionRecordModel driverMobileAppActionRecordData,
			IUnitOfWork unitOfWork
			)
		{
			this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this._orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			this._routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			this._orderConverter = orderConverter ?? throw new ArgumentNullException(nameof(orderConverter));
			this._orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			this._webApiParametersProvider = webApiParametersProvider ?? throw new ArgumentNullException(nameof(webApiParametersProvider));
			this._complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			this._aPISmsPaymentData = aPISmsPaymentData ?? throw new ArgumentNullException(nameof(aPISmsPaymentData));
			this._driverMobileAppActionRecordData = driverMobileAppActionRecordData ?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordData));
			this._unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		/// <summary>
		/// Получение заказа в требуемом формате из заказа программы ДВ (использует функцию ниже)
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>APIOrder</returns>
		public OrderDto Get(int orderId)
		{
			var order = _orderRepository.GetOrder(_unitOfWork, orderId)
				?? throw new DataNotFoundException(nameof(orderId), $"Заказ {orderId} не найден");

			return _orderConverter.convertToAPIOrder(order, _aPISmsPaymentData.GetOrderPaymentStatus(orderId));
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

			foreach (var vodovozOrder in vodovozOrders)
			{
				var smsPaymentStatus = _aPISmsPaymentData.GetOrderPaymentStatus(vodovozOrder.Id);
				var order = _orderConverter.convertToAPIOrder(vodovozOrder, smsPaymentStatus);
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
				?? throw new DataNotFoundException(nameof(orderId), $"Заказ {orderId} не найден");

			return GetAvailableToChangePaymentTypes(vodovozOrder);
		}

		/// <summary>
		/// Получение типов оплаты на которые можно изменить тип оплаты заказа переданного в аргументе
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <returns>IEnumerable APIPaymentType</returns>
		public IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(Vodovoz.Domain.Orders.Order order)
		{
			var availablePaymentTypes = new List<PaymentDtoType>();

			if (order.PaymentType == PaymentType.cash)
			{
				availablePaymentTypes.Add(PaymentDtoType.Terminal);
			}

			if (order.PaymentType == PaymentType.Terminal)
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
				?? throw new DataNotFoundException(nameof(orderId), $"Заказ {orderId} не найден");

			return GetAdditionalInfo(vodovozOrder);
		}

		/// <summary>
		/// Получение дополнительной информации для заказа из заказа программы ДВ
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <returns>APIOrderAdditionalInfo</returns>
		public OrderAdditionalInfoDto GetAdditionalInfo(Vodovoz.Domain.Orders.Order order)
		{
			return new OrderAdditionalInfoDto()
			{
				AvailablePaymentTypes = GetAvailableToChangePaymentTypes(order),
				CanSendSms = CanSendSmsForPayment(order, _aPISmsPaymentData.GetOrderPaymentStatus(order.Id)),
			};
		}

		/// <summary>
		/// Проверка возможности отправки СМС для оплаты
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <param name="smsPaymentStatus">Статус оплаты СМС</param>
		/// <returns></returns>
		private bool CanSendSmsForPayment(Vodovoz.Domain.Orders.Order order, SmsPaymentStatus? smsPaymentStatus)
		{
			return order.PaymentType == PaymentType.ByCard
				&& order.PaymentByCardFrom.Id == _orderParametersProvider.PaymentByCardFromSmsId
				&& smsPaymentStatus != SmsPaymentStatus.Paid;
		}

		public void ChangeOrderPaymentType(int orderId, PaymentType paymentType)
		{
			var vodovozOrder = _orderRepository.GetOrder(_unitOfWork, orderId)
				?? throw new DataNotFoundException(nameof(orderId), $"Заказ {orderId} не найден");

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
			DateTime actionTime)
		{
			var vodovozOrder = _orderRepository.GetOrder(_unitOfWork, orderId);
			var routeList = _routeListRepository.GetRouteListByOrder(_unitOfWork, vodovozOrder);
			var routeListAddress = routeList.Addresses.Where(x => x.Order.Id == orderId).SingleOrDefault();

			routeListAddress.DriverBottlesReturned = bottlesReturnCount;

			if(vodovozOrder == null)
			{
				var error = $"Попытка завершения несуществующего заказа: {orderId}";
				_logger.LogWarning(error);
				throw new ArgumentOutOfRangeException(nameof(orderId), error);
			}

			if(routeListAddress.Status == RouteListItemStatus.Transfered)
			{
				var error = $"Попытка завершения заказа, который был передан: {orderId}";
				_logger.LogWarning(error);
				throw new InvalidOperationException(error);
			}

			routeListAddress.UpdateStatus(_unitOfWork, RouteListItemStatus.Completed);

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
					ComplaintText = $"Заказ номер {orderId}\n" +
						$"По причине {reason}"
				};

				_unitOfWork.Save(complaint);
			}

			if (routeList.Status == RouteListStatus.EnRoute && routeList.Addresses.All(a => a.Status != RouteListItemStatus.EnRoute))
			{
				routeList.ChangeStatus(RouteListStatus.Delivered);
				_unitOfWork.Save(routeList);
			}

			_unitOfWork.Save(routeListAddress);
			_unitOfWork.Commit();

			_driverMobileAppActionRecordData.RegisterAction(
				driver,
				new DriverActionDto()
				{
					ActionType = ActionDtoType.CompleteOrderClicked,
					ActionTime = actionTime
				});
		}
	}
}
