using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.Converters;
using CustomerOrdersApi.Library.V7.Dto.Orders;
using CustomerOrdersApi.Library.V7.Services;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Orders.Services;
using Vodovoz.Core.Data.InfoMessages;
using Vodovoz.Core.Domain.Mango;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Mango;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Factories
{
	public class CustomerOrderFactory : ICustomerOrderFactory
	{
		private readonly IExternalOrderStatusConverter _externalOrderStatusConverter;
		private readonly IInfoMessageFactory _infoMessageFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly ICustomerOrderCancellationService _orderCancellationLogicService;
		private readonly ICustomerOrderTransferService _orderTransferService;
		private readonly IMangoSettings _mangoSettings;
		private readonly IOptionsMonitor<CourierCoordinatesOptions> _courierCoordinatesOptions;
		private readonly IOnlineOrderItemDtoFactory _onlineOrderItemFactory;

		public CustomerOrderFactory(
			IExternalOrderStatusConverter externalOrderStatusConverter,
			IInfoMessageFactory infoMassageFactory,
			IOrderRepository orderRepository,
			ICustomerOrderCancellationService orderCancellationLogicService,
			ICustomerOrderTransferService orderTransferService,
			IMangoSettings mangoSettings,
			IOptionsMonitor<CourierCoordinatesOptions> courierCoordinatesOptions,
			IOnlineOrderItemDtoFactory onlineOrderItemFactory
			)
		{
			_externalOrderStatusConverter =
				externalOrderStatusConverter ?? throw new ArgumentNullException(nameof(externalOrderStatusConverter));
			_infoMessageFactory = infoMassageFactory ?? throw new ArgumentNullException(nameof(infoMassageFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_orderCancellationLogicService = orderCancellationLogicService ?? throw new ArgumentNullException(nameof(orderCancellationLogicService));
			_orderTransferService = orderTransferService ?? throw new ArgumentNullException(nameof(orderTransferService));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
			_courierCoordinatesOptions = courierCoordinatesOptions ?? throw new ArgumentNullException(nameof(courierCoordinatesOptions));
			_onlineOrderItemFactory = onlineOrderItemFactory ?? throw new ArgumentNullException(nameof(onlineOrderItemFactory));
		}

		public async Task<DetailedOrderInfoDto> CreateDetailedOrderInfo(
			IUnitOfWork uow,
			Order order,
			OrderRating orderRating,
			OnlineOrderTimers timers,
			OnlineOrder onlineOrder,
			DateTime ratingAvailableFrom,
			DriverMangoExtensionNumber driversMangoExtensionNumber,
			bool establishedRoute,
			bool isOrderWasSelectedAsNext,
			DateTime? driversCoordinatesLastUpdateTime,
			CancellationToken cancellationToken
		)
		{
			var orderInfo = await CreateOrderInfoDto(
				uow,
				order,
				orderRating,
				timers,
				onlineOrder,
				ratingAvailableFrom,
				driversMangoExtensionNumber,
				establishedRoute,
				isOrderWasSelectedAsNext,
				driversCoordinatesLastUpdateTime,
				cancellationToken);

			return orderInfo;
		}

		public async Task<DetailedOrderInfoDto> CreateDetailedOrderInfo(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			OrderRating orderRating,
			OnlineOrderTimers timers,
			int? orderId,
			DateTime ratingAvailableFrom,
			CancellationToken cancellationToken
		)
		{
			var orderInfo = await CreateOrderInfoDto(
				uow,
				onlineOrder,
				orderRating,
				timers,
				orderId,
				ratingAvailableFrom,
				cancellationToken);
			
			return orderInfo;
		}

		public ActiveOrderDto CreateActiveOrderInfo(
			OrderDto orderDto,
			bool establishedRoute,
			bool isOrderWasSelectedAsNext,
			DateTime? driversCoordinatesLastUpdateTime
			)
		{
			var activeOrder = new ActiveOrderDto
			{
				OrderId = orderDto.OrderId,
				OnlineOrderId = orderDto.OnlineOrderId,
				CreatedDateTimeUtc = orderDto.CreatedDateTimeUtc,
				DeliveryDate = orderDto.DeliveryDate,
				IsSelfDelivery = orderDto.IsSelfDelivery,
				OrderSum = orderDto.OrderSum,
				OrderStatus = orderDto.OrderStatus,
				OrderPaymentStatus = orderDto.OrderPaymentStatus,
				DeliverySchedule = orderDto.DeliverySchedule,
				DeliveryAddress = orderDto.DeliveryAddress,
				RatingValue = orderDto.RatingValue,
				IsRatingAvailable = orderDto.IsRatingAvailable,
				IsNeedPay = orderDto.IsNeedPay,
				DeliveryPointId = orderDto.DeliveryPointId,
				InfoMessages = orderDto.InfoMessages
			};

			activeOrder.UpdateTrackingAvailability(
				establishedRoute,
				driversCoordinatesLastUpdateTime,
				_courierCoordinatesOptions.CurrentValue.TrackingLostTimeout
				);
			
			activeOrder.UpdateTextStatusMessage(establishedRoute, isOrderWasSelectedAsNext);

			return activeOrder;
		}

		public IEnumerable<OrderRatingReasonDto> GetOrderRatingReasonDtos(IEnumerable<OrderRatingReason> orderRatingReasons)
		{
			return orderRatingReasons.Select(x => new OrderRatingReasonDto
			{
				OrderRatingReasonId = x.Id,
				Name = x.Name,
				IsArchive = x.IsArchive,
				Ratings = x.GetRatingsArray()
			});
		}

		private async Task<DetailedOrderInfoDto> CreateOrderInfoDto(
			IUnitOfWork uow,
			Order order,
			OrderRating orderRating,
			OnlineOrderTimers timers,
			OnlineOrder onlineOrder,
			DateTime ratingAvailableFrom,
			DriverMangoExtensionNumber driversMangoExtensionNumber,
			bool establishedRoute,
			bool isOrderWasSelectedAsNext,
			DateTime? driversCoordinatesLastUpdateTime,
			CancellationToken cancellationToken
			)
		{
			var orderInfo = new DetailedOrderInfoDto
			{
				OrderId = order.Id,
				OnlineOrderId = onlineOrder?.Id,
				CreatedDateTimeUtc = order.CreateDate.HasValue ? DateTimeOffset.Parse(order.CreateDate.ToString()) : default,
				DeliveryDate = order.DeliveryDate ?? default,
				IsFastDelivery = order.IsFastDelivery,
				IsSelfDelivery = order.SelfDelivery,
				OrderSum = order.OrderSum,
				OrderStatus = _externalOrderStatusConverter.ConvertOrderStatus(order.OrderStatus),
				OnlinePaymentSource = null,
				OnlinePaymentType = null,
				//при выставленном заказе не нужны сообщения и передача таймера
				InfoMessages = Array.Empty<InfoMessage>()
			};

			if(!order.SelfDelivery)
			{
				var deliveryPoint = order.DeliveryPoint;

				if(deliveryPoint != null)
				{
					orderInfo.DeliveryPointId = deliveryPoint.Id;
					orderInfo.DeliveryAddress = deliveryPoint.ShortAddress;
				}

				orderInfo.DeliverySchedule = orderInfo.IsFastDelivery
					? DeliverySchedule.FastDelivery
					: order.DeliverySchedule?.DeliveryTime;
			}

			UpdateAvailabilityRepeatOrder(orderInfo);
			UpdateOrderRating(orderRating, ratingAvailableFrom, orderInfo);
			UpdateOrderItems(order, orderInfo);
			
			orderInfo.UpdateTrackingAvailability(
				establishedRoute,
				driversCoordinatesLastUpdateTime,
				_courierCoordinatesOptions.CurrentValue.TrackingLostTimeout
				);
			
			orderInfo.UpdateTextStatusMessage(establishedRoute, isOrderWasSelectedAsNext);

			await UpdateAvailableOperations(uow, orderInfo, order, onlineOrder, cancellationToken);

			if(driversMangoExtensionNumber != null
				&& driversMangoExtensionNumber.Status == DriverMangoExtensionNumberStatus.Active)
			{
				orderInfo.DriversMangoNumber =
					_mangoSettings.DriversCallsLineNumber + ",," + driversMangoExtensionNumber.ExtensionNumber;
			}

			return orderInfo;
		}

		private async Task<DetailedOrderInfoDto> CreateOrderInfoDto(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			OrderRating orderRating,
			OnlineOrderTimers timers,
			int? orderId,
			DateTime ratingAvailableFrom,
			CancellationToken cancellationToken
			)
		{
			var orderInfo = new DetailedOrderInfoDto
			{
				OrderId = orderId,
				OnlineOrderId = onlineOrder.Id,
				CreatedDateTimeUtc = DateTimeOffset.Parse(onlineOrder.Created.ToString()),
				DeliveryDate = onlineOrder.DeliveryDate,
				IsFastDelivery = onlineOrder.IsFastDelivery,
				IsSelfDelivery = onlineOrder.IsSelfDelivery,
				OrderSum = onlineOrder.OnlineOrderSum,
				OrderStatus = _externalOrderStatusConverter.ConvertOnlineOrderStatus(onlineOrder.OnlineOrderStatus),
				OnlinePaymentSource = onlineOrder.OnlinePaymentSource,
				OnlinePaymentType = onlineOrder.OnlineOrderPaymentType
			};

			if(timers != null)
			{
				var payTime = orderInfo.IsFastDelivery
					? (int)timers.PayTimeWithFastDelivery.TotalSeconds
					: (int)timers.PayTimeWithoutFastDelivery.TotalSeconds;

				var toManualProcessingTime = orderInfo.IsFastDelivery
					? (int)timers.TimeForTransferToManualProcessingWithFastDelivery.TotalSeconds
					: (int)timers.TimeForTransferToManualProcessingWithoutFastDelivery.TotalSeconds;

				if(onlineOrder.IsNeedOnlinePayment(payTime))
				{
					orderInfo.TimerForPaySeconds = payTime;
					orderInfo.IsNeedPay = true;
					orderInfo.InfoMessages = new[] { _infoMessageFactory.CreateNeedPayOrderInfoMessage() };
				}
				else if(onlineOrder.IsNeedOnlinePaymentButTimeIsUp(payTime, toManualProcessingTime))
				{
					orderInfo.InfoMessages = new[] { _infoMessageFactory.CreateNotPaidOrderInfoMessage() };
				}
				else
				{
					orderInfo.InfoMessages = Array.Empty<InfoMessage>();
				}
			}

			if(!onlineOrder.IsSelfDelivery)
			{
				var deliveryPoint = onlineOrder.DeliveryPoint;

				if(deliveryPoint != null)
				{
					orderInfo.DeliveryPointId = deliveryPoint.Id;
					orderInfo.DeliveryAddress = deliveryPoint.ShortAddress;
				}

				orderInfo.DeliverySchedule = orderInfo.IsFastDelivery
					? DeliverySchedule.FastDelivery
					: onlineOrder.DeliverySchedule?.DeliveryTime;
			}

			UpdateAvailabilityRepeatOrder(orderInfo);
			UpdateOrderRating(orderRating, ratingAvailableFrom, orderInfo);
			UpdateOrderItems(onlineOrder, orderInfo);

			var activeOrder = GetActiveOrder(onlineOrder);
			await UpdateAvailableOperations(uow, orderInfo, activeOrder, onlineOrder, cancellationToken);

			return orderInfo;
		}

		private void UpdateAvailabilityRepeatOrder(DetailedOrderInfoDto orderInfo)
		{
			if(orderInfo.OrderStatus is ExternalOrderStatus.OrderCompleted or ExternalOrderStatus.Canceled)
			{
				orderInfo.AvailableRepeatOrder = true;
			}
		}

		/// <summary>
		/// Получает активный заказ из онлайн-заказа
		/// </summary>
		private Order GetActiveOrder(OnlineOrder onlineOrder)
		{
			var availableStatuses = _orderRepository.GetStatusesForTransferOrCancellationOnlineOrder();
			return onlineOrder.Orders?.FirstOrDefault(x => availableStatuses.Contains(x.OrderStatus));
		}

		/// <summary>
		/// Обновляет доступность операций (отмена, перенос) и добавляет информационные сообщения
		/// </summary>
		private async Task UpdateAvailableOperations(
			IUnitOfWork uow,
			DetailedOrderInfoDto orderInfo,
			Order order,
			OnlineOrder onlineOrder,
			CancellationToken cancellationToken
		)
		{
			var cancelResult = await _orderCancellationLogicService.CanCancel(
				uow, 
				order, 
				onlineOrder, 
				cancellationToken
			);

			orderInfo.AvailableCancelOrder = cancelResult.IsSuccess;

			if(orderInfo.AvailableCancelOrder && (order is not null || onlineOrder is not null))
			{
				AddCancelOrderInfoMessage(orderInfo, order, onlineOrder);
			}

			if(order is not null)
			{
				var transferResult = _orderTransferService.CanTransfer(order);
				orderInfo.AvailableChangeDeliverySchedule = transferResult.IsSuccess;
			}
			else
			{
				orderInfo.AvailableChangeDeliverySchedule = false;
			}
		}

		/// <summary>
		/// Добавляет информационное сообщение об отмене для оплаченных заказов
		/// </summary>
		private void AddCancelOrderInfoMessage(
			DetailedOrderInfoDto orderInfo,
			Order order,
			OnlineOrder onlineOrder)
		{
			var isPaid = false;

			if(order is not null)
			{
				isPaid = order.PaymentType is PaymentType.PaidOnline;
			}
			else if(onlineOrder is not null)
			{
				isPaid = onlineOrder.OnlineOrderPaymentStatus is OnlineOrderPaymentStatus.Paid;
			}

			if(isPaid)
			{
				var existingMessages = orderInfo.InfoMessages?.ToList() ?? new List<InfoMessage>();
				existingMessages.Add(_infoMessageFactory.CreateRefundPaymentInfoMessage());
				orderInfo.InfoMessages = existingMessages;
			}
		}
		
		private void UpdateOrderRating(
			OrderRating orderRating,
			DateTime ratingAvailableFrom,
			DetailedOrderInfoDto orderInfoDto)
		{
			if(orderRating is null)
			{
				orderInfoDto.IsRatingAvailable =
					orderInfoDto.CreatedDateTimeUtc >= DateTimeOffset.Parse(ratingAvailableFrom.ToString())
					&& (orderInfoDto.OrderStatus == ExternalOrderStatus.OrderCompleted
						|| orderInfoDto.OrderStatus == ExternalOrderStatus.Canceled
						|| orderInfoDto.OrderStatus == ExternalOrderStatus.OrderDelivering);
				orderInfoDto.RatingReasonsIds = new List<int>();
				return;
			}

			orderInfoDto.RatingReasonsIds = orderRating.OrderRatingReasons.Select(x => x.Id).ToList();
			orderInfoDto.OrderRatingComment = orderRating.Comment;
			orderInfoDto.RatingValue = orderRating.Rating;
			orderInfoDto.IsRatingAvailable = false;
		}
		
		private void UpdateOrderItems(Order order, DetailedOrderInfoDto orderInfoDto)
		{
			orderInfoDto.OrderItems = order.OrderItems
				.Where(x => x.PromoSet is null)
				.Select(_onlineOrderItemFactory.CreateWithDiscountDetailsDto)
				.ToList();

			AddPromoSets(order.PromotionalSets, orderInfoDto);
		}
		
		private void UpdateOrderItems(OnlineOrder onlineOrder, DetailedOrderInfoDto orderInfoDto)
		{
			orderInfoDto.OrderItems = onlineOrder.OnlineOrderItems
				.Where(x => x.PromoSet is null)
				.Select(_onlineOrderItemFactory.CreateWithDiscountDetailsDto)
				.ToList();

			if(onlineOrder is OnlineOrderV2 onlineOrderV2)
			{
				AddPromoSets(onlineOrderV2.PromoSets, orderInfoDto);
			}
			else
			{
				AddPromoSets(onlineOrder.OnlineOrderItems, orderInfoDto);
			}

			AddRentPackages(onlineOrder.OnlineRentPackages, orderInfoDto);
		}
		
		private void AddPromoSets(IEnumerable<IProduct> orderItems, DetailedOrderInfoDto orderInfoDto)
		{
			var promoSetsGroup = orderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSet.Id);
			
			foreach(var orderItemGroup in promoSetsGroup)
			{
				var promo = orderItemGroup.First().PromoSet;
				var promoItemsCount = promo.PromotionalSetItems.Count;
					
				orderInfoDto.OrderItems.Add(_onlineOrderItemFactory.CreateWithDiscountDetailsDto(promo, orderItemGroup.Count() / promoItemsCount));
			}
		}
		
		private void AddPromoSets(IEnumerable<OnlineOrderPromoSet> onlineOrderPromoSets, DetailedOrderInfoDto orderInfoDto)
		{
			foreach(var onlineOrderPromoSet in onlineOrderPromoSets)
			{
				orderInfoDto.OrderItems.Add(
					_onlineOrderItemFactory.CreateWithDiscountDetailsDto(onlineOrderPromoSet.PromoSet, onlineOrderPromoSet.Count)
					);
			}
		}
		
		private void AddPromoSets(IEnumerable<PromotionalSet> promoSets, DetailedOrderInfoDto orderInfoDto)
		{
			var onlineOrderItemPromoSets = _onlineOrderItemFactory.CreateWithDiscountDetailsDto(promoSets);
			
			foreach(var promoSet in onlineOrderItemPromoSets)
			{
				orderInfoDto.OrderItems.Add(promoSet);
			}
		}
		
		private void AddRentPackages(IEnumerable<OnlineFreeRentPackage> freeRentPackages, DetailedOrderInfoDto orderInfoDto)
		{
			foreach(var freeRentPackage in freeRentPackages)
			{
				orderInfoDto.OrderItems.Add(_onlineOrderItemFactory.CreateWithDiscountDetailsDto(freeRentPackage));
			}
		}
	}
}
