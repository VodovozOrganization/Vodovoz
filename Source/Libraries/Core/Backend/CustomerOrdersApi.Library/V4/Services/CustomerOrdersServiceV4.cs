using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using CustomerOrdersApi.Library.V4.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders.V4;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Services.Orders;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V4.Services
{
	public class CustomerOrdersServiceV4 : ICustomerOrdersServiceV4
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ILogger<CustomerOrdersServiceV4> _logger;
		private readonly ISignatureManager _signatureManager;
		private readonly ICustomerOrderFactoryV4 _customerOrderFactory;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IGenericRepository<OrderRating> _genericRatingRepository;
		private readonly IUnPaidOnlineOrderHandler _unPaidOnlineOrderHandler;
		private readonly IConfigurationSection _signaturesSection;

		public CustomerOrdersServiceV4(
			IUnitOfWorkFactory unitOfWorkFactory,
			ILogger<CustomerOrdersServiceV4> logger,
			ISignatureManager signatureManager,
			ICustomerOrderFactoryV4 customerOrderFactory,
			IOrderSettings orderSettings,
			IOrderRepository orderRepository,
			IOnlineOrderRepository onlineOrderRepository,
			IGenericRepository<OrderRating> genericRatingRepository,
			IUnPaidOnlineOrderHandler unPaidOnlineOrderHandler,
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
			_unPaidOnlineOrderHandler = unPaidOnlineOrderHandler ?? throw new ArgumentNullException(nameof(unPaidOnlineOrderHandler));

			_signaturesSection = configuration.GetSection("Signatures");
		}

		#region ValidateSignature

		public bool ValidateOrderSignature(ICreatingOnlineOrder creatingOnlineOrder, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(creatingOnlineOrder.Source);
			
			return _signatureManager.Validate(
				creatingOnlineOrder.Signature,
				new OrderSignatureParams
				{
					OrderId = creatingOnlineOrder.ExternalOrderId.ToString(),
					OrderSumInKopecks = (int)(creatingOnlineOrder.OrderSum * 100),
					ShopId = (int)creatingOnlineOrder.Source,
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
			var timers = uow.GetAll<OnlineOrderTimers>().FirstOrDefault();

			if(getDetailedOrderInfoDto.OrderId.HasValue)
			{
				var order = uow.GetById<Order>(getDetailedOrderInfoDto.OrderId.Value);
				
				orderRating = _genericRatingRepository.Get(
						uow,
						x => x.Order.Id == order.Id)
					.FirstOrDefault();
			
				return _customerOrderFactory.CreateDetailedOrderInfo(
					order, orderRating, timers, getDetailedOrderInfoDto.OnlineOrderId, ratingAvailableFrom);
			}
			
			var onlineOrder = uow.GetById<OnlineOrder>(getDetailedOrderInfoDto.OnlineOrderId.Value);
			orderRating = _genericRatingRepository.Get(
					uow,
					x => x.OnlineOrder.Id == onlineOrder.Id)
				.FirstOrDefault();
			
			return _customerOrderFactory.CreateDetailedOrderInfo(
				onlineOrder, orderRating, timers, getDetailedOrderInfoDto.OrderId, ratingAvailableFrom);
		}

		public OrdersDto GetOrders(GetOrdersDto getOrdersDto)
		{
			var skipElements = (getOrdersDto.Page - 1) * getOrdersDto.OrdersCountOnPage;
			var dateAvailabilityRating = _orderSettings.GetDateAvailabilityRatingOrder;
			
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			var ordersWithoutOnlineOrders =
				_orderRepository.GetCounterpartyOrdersWithoutOnlineOrdersV4(uow, getOrdersDto.CounterpartyErpId, dateAvailabilityRating);
			var onlineOrdersWithOrders = GetOnlineOrdersWithOrdersInfo(getOrdersDto, uow, dateAvailabilityRating);
			var onlineOrdersWithoutOrders =
				_onlineOrderRepository.GetCounterpartyOnlineOrdersWithoutOrderV4(uow, getOrdersDto.CounterpartyErpId, dateAvailabilityRating);

			var allOrders = 
				ordersWithoutOnlineOrders
					.Concat(onlineOrdersWithOrders)
					.Concat(onlineOrdersWithoutOrders)
					.ToArray();

			var res = allOrders
					.OrderByDescending(x => x.DeliveryDate)
					.ThenByDescending(x => x.CreatedDateTimeUtc)
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
				_orderRepository.GetCounterpartyOrdersFromOnlineOrdersV4(uow, getOrdersDto.CounterpartyErpId, dateAvailabilityRating)
					.ToLookup(x => x.OnlineOrderId);

			foreach(var ordersFromOnlineOrdersGroup in ordersFromOnlineOrders)
			{
				OrderDto onlineOrderInfo;
				if(ordersFromOnlineOrdersGroup.Count() == 1)
				{
					var orderDto = ordersFromOnlineOrdersGroup.First();
					onlineOrderInfo = new OrderDto
					{
						OnlineOrderId = orderDto.OnlineOrderId,
						DeliveryDate = orderDto.DeliveryDate,
						CreatedDateTimeUtc = orderDto.CreatedDateTimeUtc,
						DeliveryAddress = orderDto.DeliveryAddress,
						DeliverySchedule = orderDto.DeliverySchedule,
						RatingValue = orderDto.RatingValue,
						IsRatingAvailable = orderDto.IsRatingAvailable,
						IsNeedPay = false,
						DeliveryPointId = orderDto.DeliveryPointId,
						OrderSum = orderDto.OrderSum,
						OrderStatus = orderDto.OrderStatus
					};

					onlineOrdersInfo.Add(onlineOrderInfo);
				}
				else
				{
					foreach(var orderDto in ordersFromOnlineOrdersGroup)
					{
						onlineOrderInfo = new OrderDto
						{
							OrderId = orderDto.OrderId,
							DeliveryDate = orderDto.DeliveryDate,
							CreatedDateTimeUtc = orderDto.CreatedDateTimeUtc,
							DeliveryAddress = orderDto.DeliveryAddress,
							DeliverySchedule = orderDto.DeliverySchedule,
							RatingValue = orderDto.RatingValue,
							IsRatingAvailable = orderDto.IsRatingAvailable,
							IsNeedPay = false,
							DeliveryPointId = orderDto.DeliveryPointId,
							OrderSum = orderDto.OrderSum,
							OrderStatus = orderDto.OrderStatus
						};
						onlineOrdersInfo.Add(onlineOrderInfo);
					}
				}
			}
			
			return onlineOrdersInfo;
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

		public (int HttpCode, string Message, AvailablePaymentMethods AvailablePayments) GetAvailablePaymentMethods(
			GetAvailablePaymentMethodsDto getAvailablePaymentMethods)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис онлайн заказов. Получение доступных способов оплат");
			var onlineOrder = _onlineOrderRepository.GetOnlineOrderById(uow, getAvailablePaymentMethods.OnlineOrderId);
			
			var result = _unPaidOnlineOrderHandler.CanChangePaymentType(uow, onlineOrder);
			
			if(result.IsFailure)
			{
				var firstError = result.Errors.First();
				return (int.Parse(firstError.Code), firstError.Message, null);
			}
			
			var availablePayments = AvailablePaymentMethods.Create(getAvailablePaymentMethods.Source, onlineOrder.OnlineOrderPaymentType);
			
			return (200, null, availablePayments);
		}

		public async Task<Result<ChangedOrderDto>> UpdateOrderAsync(ChangingOrderDto changingOrderDto, CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис онлайн заказов. Изменение заказа");
			
			var orders = _orderRepository.GetOrdersFromOnlineOrder(uow, changingOrderDto.OnlineOrderId ?? 0);
			var onlineOrder = uow.GetAll<OnlineOrder>().FirstOrDefault(x => x.Id == changingOrderDto.OnlineOrderId);
			var deliverySchedule = uow.GetAll<Vodovoz.Domain.Logistic.DeliverySchedule>()
				.FirstOrDefault(x => x.Id == changingOrderDto.DeliveryScheduleId);

			var result = await _unPaidOnlineOrderHandler.TryUpdateOrderAsync(
				uow,
				orders,
				onlineOrder,
				deliverySchedule,
				changingOrderDto.ToUpdateOnlineOrderFromChangeRequest(),
				cancellationToken);
			
			if(result.IsFailure)
			{
				return Result.Failure<ChangedOrderDto>(result.Errors);
			}
			
			var changedOrderDto = ChangedOrderDto.Create(changingOrderDto.OnlineOrderId);
			
			return Result.Success(changedOrderDto);
		}

		private string GetSourceSign(Source source)
		{
			return _signaturesSection.GetValue<string>(source.ToString());
		}
	}
}
