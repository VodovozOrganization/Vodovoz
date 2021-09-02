using DriverAPI.Library.Converters;
using DriverAPI.Library.DTOs;
using DriverAPI.Library.Helpers;
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
		private readonly OrderConverter _orderConverter;
		private readonly IDriverApiParametersProvider _webApiParametersProvider;
		private readonly IComplaintsRepository _complaintsRepository;
		private readonly ISmsPaymentModel _aPISmsPaymentModel;
		private readonly ISmsPaymentServiceAPIHelper _smsPaymentServiceAPIHelper;
		private readonly IUnitOfWork _unitOfWork;

		private readonly int _maxClosingRating = 5;
		private readonly PaymentType[] _smsNotPayable = new PaymentType[] { PaymentType.ByCard, PaymentType.barter, PaymentType.ContractDoc };

		public OrderModel(ILogger<OrderModel> logger,
			IOrderRepository orderRepository,
			IRouteListRepository routeListRepository,
			OrderConverter orderConverter,
			IDriverApiParametersProvider webApiParametersProvider,
			IComplaintsRepository complaintsRepository,
			ISmsPaymentModel aPISmsPaymentModel,
			ISmsPaymentServiceAPIHelper smsPaymentServiceAPIHelper,
			IUnitOfWork unitOfWork
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_orderConverter = orderConverter ?? throw new ArgumentNullException(nameof(orderConverter));
			_webApiParametersProvider = webApiParametersProvider ?? throw new ArgumentNullException(nameof(webApiParametersProvider));
			_complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			_aPISmsPaymentModel = aPISmsPaymentModel ?? throw new ArgumentNullException(nameof(aPISmsPaymentModel));
			_smsPaymentServiceAPIHelper = smsPaymentServiceAPIHelper ?? throw new ArgumentNullException(nameof(smsPaymentServiceAPIHelper));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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

			var order = _orderConverter.convertToAPIOrder(vodovozOrder, _aPISmsPaymentModel.GetOrderPaymentStatus(orderId));
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
				var smsPaymentStatus = _aPISmsPaymentModel.GetOrderPaymentStatus(vodovozOrder.Id);
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
			return new OrderAdditionalInfoDto()
			{
				AvailablePaymentTypes = GetAvailableToChangePaymentTypes(order),
				CanSendSms = CanSendSmsForPayment(order, _aPISmsPaymentModel.GetOrderPaymentStatus(order.Id)),
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
			return !_smsNotPayable.Contains(order.PaymentType)
				&& order.OrderTotalSum > 0;
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
			DateTime recievedTime)
		{
			var vodovozOrder = _orderRepository.GetOrder(_unitOfWork, orderId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_unitOfWork, vodovozOrder);
			var routeListAddress = routeList.Addresses.Where(x => x.Order.Id == orderId).SingleOrDefault();

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

			routeListAddress.DriverBottlesReturned = bottlesReturnCount;

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning($"Водитель {driver.Id} попытался завершить заказ {orderId} водителя {routeList.Driver.Id}");
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
					CreationDate = recievedTime,
					ChangedDate = recievedTime,
					CreatedBy = driver,
					ChangedBy = driver,
					ComplaintText = $"Заказ номер { orderId }\n" +
						$"По причине { reason }"
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
		}

		public void SendSmsPaymentRequest(
			int orderId,
			string phoneNumber,
			int driverId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_unitOfWork, orderId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_unitOfWork, vodovozOrder);
			var routeListAddress = routeList.Addresses.Where(x => x.Order.Id == orderId).FirstOrDefault();

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
				_logger.LogWarning($"Водитель {driverId} попытался запросить оплату по СМС для заказа {orderId} водителя {routeList.Driver.Id}");
				throw new InvalidOperationException("Нельзя запросить оплату по СМС для заказа другого водителя");
			}

			_smsPaymentServiceAPIHelper.SendPayment(orderId, phoneNumber).Wait();
		}
	}
}
