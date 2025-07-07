﻿using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.Services
{
	public class CustomerOrdersService : ICustomerOrdersService
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ILogger<CustomerOrdersService> _logger;
		private readonly ISignatureManager _signatureManager;
		private readonly ICustomerOrderFactory _customerOrderFactory;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IGenericRepository<OrderRating> _genericRatingRepository;
		private readonly IConfigurationSection _signaturesSection;

		public CustomerOrdersService(
			IUnitOfWorkFactory unitOfWorkFactory,
			ILogger<CustomerOrdersService> logger,
			ISignatureManager signatureManager,
			ICustomerOrderFactory customerOrderFactory,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IOnlineOrderRepository onlineOrderRepository,
			IGenericRepository<OrderRating> genericRatingRepository,
			IConfiguration configuration)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_customerOrderFactory = customerOrderFactory ?? throw new ArgumentNullException(nameof(customerOrderFactory));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_genericRatingRepository = genericRatingRepository ?? throw new ArgumentNullException(nameof(genericRatingRepository));

			_signaturesSection = configuration.GetSection("Signatures");
		}

		#region ValidateSignature

		public bool ValidateOrderSignature(OnlineOrderInfoDto onlineOrderInfoDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(onlineOrderInfoDto.Source);
			
			return _signatureManager.Validate(
				onlineOrderInfoDto.Signature,
				new OrderSignatureParams
				{
					OrderId = onlineOrderInfoDto.ExternalOrderId.ToString(),
					OrderSumInKopecks = (int)(onlineOrderInfoDto.OrderSum * 100),
					ShopId = (int)onlineOrderInfoDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}
		
		public bool ValidateOrderRatingSignature(OrderRatingInfoForCreateDto orderRatingInfo, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(orderRatingInfo.Source);
			
			return _signatureManager.Validate(
				orderRatingInfo.Signature,
				new OrderRatingSignatureParams
				{
					OrderNumber = orderRatingInfo.OrderId.HasValue
						? orderRatingInfo.OrderId.ToString()
						: orderRatingInfo.OnlineOrderId.ToString(),
					Rating = orderRatingInfo.Rating,
					ShopId = (int)orderRatingInfo.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}
		
		public bool ValidateOrderInfoSignature(GetDetailedOrderInfoDto getDetailedOrderInfoDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(getDetailedOrderInfoDto.Source);
			
			return _signatureManager.Validate(
				getDetailedOrderInfoDto.Signature,
				new OrderInfoSignatureParams
				{
					OrderId = getDetailedOrderInfoDto.OrderId.HasValue
						? getDetailedOrderInfoDto.OrderId.ToString()
						: getDetailedOrderInfoDto.OnlineOrderId.ToString(),
					ShopId = (int)getDetailedOrderInfoDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}
		
		public bool ValidateCounterpartyOrdersSignature(GetOrdersDto getOrdersDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(getOrdersDto.Source);
			
			return _signatureManager.Validate(
				getOrdersDto.Signature,
				new CounterpartyOrdersSignatureParams
				{
					CounterpartyId = getOrdersDto.CounterpartyErpId.ToString(),
					Page = getOrdersDto.Page,
					ShopId = (int)getOrdersDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}
		
		public bool ValidateOnlineOrderPaymentStatusUpdatedSignature(
			OnlineOrderPaymentStatusUpdatedDto paymentStatusUpdatedDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(paymentStatusUpdatedDto.Source);
			
			return _signatureManager.Validate(
				paymentStatusUpdatedDto.Signature,
				new OnlineOrderPaymentStatusUpdatedSignatureParams
				{
					OnlineOrderId = paymentStatusUpdatedDto.ExternalOrderId.ToString(),
					OnlinePayment = paymentStatusUpdatedDto.OnlinePayment,
					ShopId = (int)paymentStatusUpdatedDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}

		public bool ValidateRequestForCallSignature(CreatingRequestForCallDto creatingInfoDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(creatingInfoDto.Source);
			
			return _signatureManager.Validate(
				creatingInfoDto.Signature,
				new RequestForCallSignatureParams
				{
					PhoneNumber = creatingInfoDto.PhoneNumber,
					ShopId = (int)creatingInfoDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}

		#endregion

		public DetailedOrderInfoDto GetDetailedOrderInfo(GetDetailedOrderInfoDto getDetailedOrderInfoDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			
			var ratingAvailableFrom = _orderSettings.GetDateAvailabilityRatingOrder;
			OrderRating orderRating = null;

			if(getDetailedOrderInfoDto.OrderId.HasValue)
			{
				var order = uow.GetById<Order>(getDetailedOrderInfoDto.OrderId.Value);
				orderRating = _genericRatingRepository.Get(
						uow,
						x => x.Order.Id == order.Id)
					.FirstOrDefault();
			
				return _customerOrderFactory.CreateDetailedOrderInfo(
					order, orderRating, getDetailedOrderInfoDto.OnlineOrderId, ratingAvailableFrom);
			}
			
			var onlineOrder = uow.GetById<OnlineOrder>(getDetailedOrderInfoDto.OnlineOrderId.Value);
			orderRating = _genericRatingRepository.Get(
					uow,
					x => x.OnlineOrder.Id == onlineOrder.Id)
				.FirstOrDefault();
			
			return _customerOrderFactory.CreateDetailedOrderInfo(
				onlineOrder, orderRating, getDetailedOrderInfoDto.OrderId, ratingAvailableFrom);
		}

		public OrdersDto GetOrders(GetOrdersDto getOrdersDto)
		{
			var skipElements = (getOrdersDto.Page - 1) * getOrdersDto.OrdersCountOnPage;
			var dateAvailabilityRating = _orderSettings.GetDateAvailabilityRatingOrder;
			
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			var ordersWithoutOnlineOrders =
				_orderRepository.GetCounterpartyOrdersWithoutOnlineOrders(uow, getOrdersDto.CounterpartyErpId, dateAvailabilityRating);
			var onlineOrdersWithOrders = GetOnlineOrdersWithOrdersInfo(getOrdersDto, uow, dateAvailabilityRating);
			var onlineOrdersWithoutOrders =
				_onlineOrderRepository.GetCounterpartyOnlineOrdersWithoutOrder(uow, getOrdersDto.CounterpartyErpId, dateAvailabilityRating);

			var allOrders = 
				ordersWithoutOnlineOrders
					.Concat(onlineOrdersWithOrders)
					.Concat(onlineOrdersWithoutOrders)
					.ToArray();

			var res = allOrders
					.OrderByDescending(x => x.DeliveryDate)
					.ThenByDescending(x => x.CreationDate)
					.Skip(skipElements)
					.Take(getOrdersDto.OrdersCountOnPage)
					.ToArray();

			return new OrdersDto
			{
				Orders = res,
				OrdersCount = allOrders.Length
			};
		}

		private IEnumerable<OrderDto> GetOnlineOrdersWithOrdersInfo(GetOrdersDto getOrdersDto, IUnitOfWork uow, DateTime dateAvailabilityRating)
		{
			var onlineOrdersInfo = new List<OrderDto>();
			var ordersFromOnlineOrders =
				_orderRepository.GetCounterpartyOrdersFromOnlineOrders(uow, getOrdersDto.CounterpartyErpId, dateAvailabilityRating);
			
			var onlineOrderInfo = new OrderDto();
			var i = 0;

			foreach(var orderFromOnlineOrder in ordersFromOnlineOrders)
			{
				if(i == 0 || orderFromOnlineOrder.OnlineOrderId == onlineOrderInfo.OnlineOrderId)
				{
					UpdateOnlineOrderInfo(onlineOrderInfo, orderFromOnlineOrder);
					i++;
					continue;
				}
				
				onlineOrdersInfo.Add(onlineOrderInfo);
				onlineOrderInfo = new OrderDto();
				UpdateOnlineOrderInfo(onlineOrderInfo, orderFromOnlineOrder);
				i++;
			}
			
			return onlineOrdersInfo;
		}

		private void UpdateOnlineOrderInfo(OrderDto onlineOrderInfo, OrderDto orderFromOnlineOrder)
		{
			onlineOrderInfo.OnlineOrderId = orderFromOnlineOrder.OnlineOrderId;
			onlineOrderInfo.DeliveryDate = orderFromOnlineOrder.DeliveryDate;
			onlineOrderInfo.CreationDate = orderFromOnlineOrder.CreationDate;
			onlineOrderInfo.DeliveryAddress = orderFromOnlineOrder.DeliveryAddress;
			onlineOrderInfo.DeliverySchedule = orderFromOnlineOrder.DeliverySchedule;
			onlineOrderInfo.RatingValue = orderFromOnlineOrder.RatingValue;
			onlineOrderInfo.IsRatingAvailable = orderFromOnlineOrder.IsRatingAvailable;
			onlineOrderInfo.IsNeedPayment = false;
			onlineOrderInfo.DeliveryPointId = orderFromOnlineOrder.DeliveryPointId;

			UpdateOnlineOrderStatusAndSumInfo(onlineOrderInfo, orderFromOnlineOrder);
		}

		private void UpdateOnlineOrderStatusAndSumInfo(OrderDto onlineOrderInfo, OrderDto orderFromOnlineOrder)
		{
			switch(orderFromOnlineOrder.OrderStatus)
			{
				case ExternalOrderStatus.WaitingForPayment:
				case ExternalOrderStatus.OrderProcessing:
				case ExternalOrderStatus.OrderPerformed:
				case ExternalOrderStatus.OrderDelivering:
					onlineOrderInfo.OrderStatus = orderFromOnlineOrder.OrderStatus;
					break;
				case ExternalOrderStatus.OrderCollecting:
					switch(onlineOrderInfo.OrderStatus)
					{
						case ExternalOrderStatus.OrderCompleted:
							onlineOrderInfo.OrderStatus = ExternalOrderStatus.OrderDelivering;
							break;
						default:
							onlineOrderInfo.OrderStatus = orderFromOnlineOrder.OrderStatus;
							break;
					}
					break;
				case ExternalOrderStatus.OrderCompleted:
					switch(onlineOrderInfo.OrderStatus)
					{
						case ExternalOrderStatus.WaitingForPayment:
						case ExternalOrderStatus.OrderProcessing:
						case ExternalOrderStatus.OrderPerformed:
						case ExternalOrderStatus.OrderCollecting:
							onlineOrderInfo.OrderStatus = ExternalOrderStatus.OrderDelivering;
							break;
						default:
							onlineOrderInfo.OrderStatus = orderFromOnlineOrder.OrderStatus;
							break;
					}
					break;
				case ExternalOrderStatus.Canceled:
					switch(onlineOrderInfo.OrderStatus)
					{
						case ExternalOrderStatus.Canceled:
							onlineOrderInfo.OrderStatus = orderFromOnlineOrder.OrderStatus;
							break;
					}
					break;
			}
			
			if(orderFromOnlineOrder.OrderStatus != ExternalOrderStatus.Canceled)
			{
				onlineOrderInfo.OrderSum += orderFromOnlineOrder.OrderSum;
			}
		}

		public IEnumerable<OrderRatingReasonDto> GetOrderRatingReasons()
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			var reasons = uow.GetAll<OrderRatingReason>().ToArray();

			return _customerOrderFactory.GetOrderRatingReasonDtos(reasons);
		}
		
		public void CreateOrderRating(OrderRatingInfoForCreateDto orderRatingInfo)
		{
			var negativeRating = _orderSettings.GetOrderRatingForMandatoryProcessing;
			var orderRating = OrderRating.Create(
				orderRatingInfo.Source,
				orderRatingInfo.Rating,
				orderRatingInfo.Comment,
				orderRatingInfo.OnlineOrderId,
				orderRatingInfo.OrderId,
				orderRatingInfo.OrderRatingReasonsIds,
				negativeRating);
			
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			uow.Save(orderRating);
			uow.Commit();
		}
		
		public bool TryUpdateOnlineOrderPaymentStatus(OnlineOrderPaymentStatusUpdatedDto paymentStatusUpdatedDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			var onlineOrder = _onlineOrderRepository.GetOnlineOrderByExternalId(uow, paymentStatusUpdatedDto.ExternalOrderId);

			if(onlineOrder is null)
			{
				return false;
			}

			onlineOrder.OnlinePayment = paymentStatusUpdatedDto.OnlinePayment;
			onlineOrder.OnlinePaymentSource = paymentStatusUpdatedDto.OnlinePaymentSource;
			onlineOrder.OnlineOrderPaymentStatus = paymentStatusUpdatedDto.OnlineOrderPaymentStatus;
			uow.Save(onlineOrder);
			uow.Commit();
			return true;
		}

		public void CreateRequestForCall(CreatingRequestForCallDto creatingInfoDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();

			Nomenclature nomenclature = null;
			Counterparty counterparty = null;

			if(creatingInfoDto.NomenclatureErpId.HasValue)
			{
				nomenclature = uow.GetById<Nomenclature>(creatingInfoDto.NomenclatureErpId.Value);
			}
			
			if(creatingInfoDto.CounterpartyErpId.HasValue)
			{
				counterparty = uow.GetById<Counterparty>(creatingInfoDto.CounterpartyErpId.Value);
			}

			var requestForCall = RequestForCall.Create(
				creatingInfoDto.Source,
				creatingInfoDto.ContactName,
				creatingInfoDto.PhoneNumber,
				nomenclature,
				counterparty
				);
			
			uow.Save(requestForCall);
			uow.Commit();
		}

		private string GetSourceSign(Source source)
		{
			return _signaturesSection.GetValue<string>(source.ToString());
		}
	}
}
