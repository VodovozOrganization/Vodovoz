using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RobotMiaApi.Contracts.Requests.V1;
using RobotMiaApi.Contracts.Responses.V1;
using RobotMiaApi.Extensions.Mapping;
using RobotMiaApi.Specifications;
using Sms.Internal;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain.Orders;
using Vodovoz.Models;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Roboats;
using VodovozBusiness.Specifications.Orders;
using IVodovozOrderService = VodovozBusiness.Services.Orders.IOrderService;

namespace RobotMiaApi.Services
{
	/// <inheritdoc cref="IOrderService"/>
	public class OrderService : IOrderService
	{
		private static readonly OrderStatus[] _lastOrderCompletedStatuses = new OrderStatus[]
		{
			OrderStatus.Shipped,
			OrderStatus.Closed,
			OrderStatus.UnloadingOnStock
		};

		private readonly ILogger<OrderService> _logger;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IRoboatsSettings _roboatsSettings;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly IVodovozOrderService _vodovozOrderService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IFastPaymentSender _fastPaymentSender;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="nomenclatureSettings"></param>
		/// <param name="roboatsSettings"></param>
		/// <param name="orderRepository"></param>
		/// <param name="vodovozOrderService"></param>
		/// <param name="unitOfWork"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public OrderService(
			ILogger<OrderService> logger,
			INomenclatureSettings nomenclatureSettings,
			IRoboatsSettings roboatsSettings,
			IGenericRepository<Order> orderRepository,
			IVodovozOrderService vodovozOrderService,
			IUnitOfWork unitOfWork,
			IFastPaymentSender fastPaymentSender)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_nomenclatureSettings = nomenclatureSettings
				?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_roboatsSettings = roboatsSettings
				?? throw new ArgumentNullException(nameof(roboatsSettings));
			_orderRepository = orderRepository
				?? throw new ArgumentNullException(nameof(orderRepository));
			_vodovozOrderService = vodovozOrderService;
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_fastPaymentSender = fastPaymentSender
				?? throw new ArgumentNullException(nameof(fastPaymentSender));
		}


		/// <inheritdoc/>
		public LastOrderResponse GetLastOrderByCounterpartyId(int counterpartyId)
		{
			return _orderRepository
				.Get(
					_unitOfWork,
					OrderSpecification
						.CreateForCounterpartyId(counterpartyId)
						.And(LastOrderSpecification.CreateForValidRobotMiaLastOrders(
							_lastOrderCompletedStatuses,
							DateTime.Today.AddMonths(-_roboatsSettings.OrdersInMonths),
							_nomenclatureSettings.PaidDeliveryNomenclatureId,
							_nomenclatureSettings.FastDeliveryNomenclatureId)))
				.FirstOrDefault()?
				.MapToLastOrderResponseV1();
		}

		/// <inheritdoc/>
		public LastOrderResponse GetLastOrderByDeliveryPointId(int deliveryPointId)
		{
			return _orderRepository
				.Get(
					_unitOfWork,
					OrderSpecification
						.CreateForDeliveryPointId(deliveryPointId)
						.And(LastOrderSpecification.CreateForValidRobotMiaLastOrders(
							_lastOrderCompletedStatuses,
							DateTime.Today.AddMonths(-_roboatsSettings.OrdersInMonths),
							_nomenclatureSettings.PaidDeliveryNomenclatureId,
							_nomenclatureSettings.FastDeliveryNomenclatureId)))
				.FirstOrDefault()?
				.MapToLastOrderResponseV1();
		}

		/// <inheritdoc/>
		public void CreateIncompleteOrder(CreateOrderRequest createOrderRequest)
		{
			_vodovozOrderService.CreateIncompleteOrder(createOrderRequest.MapToCreateOrderRequest());
		}

		/// <inheritdoc/>
		public async Task<int> CreateAndAcceptOrder(CreateOrderRequest createOrderRequest)
		{
			var orderArgs = createOrderRequest.MapToCreateOrderRequest();

			if(createOrderRequest.PaymentType == PaymentType.SmsQR)
			{
				var orderData = _vodovozOrderService.CreateIncompleteOrder(orderArgs);

				var paymentSent = await TryingSendPayment(createOrderRequest.ContactPhone, orderData.OrderId);

				if(paymentSent)
				{
					orderData = _vodovozOrderService.AcceptOrder(orderData.OrderId, orderData.AuthorId);
				}

				return orderData.OrderId;
			}

			return _vodovozOrderService.CreateAndAcceptOrder(orderArgs);
		}

		/// <inheritdoc/>
		public (decimal orderPrice, decimal deliveryPrice, decimal forfeitPrice) GetOrderAndDeliveryPrices(CalculatePriceRequest calculatePriceRequest)
		{
			return _vodovozOrderService.GetOrderAndDeliveryPrices(calculatePriceRequest.MapToCreateOrderRequest());
		}

		private async Task<bool> TryingSendPayment(string phone, int orderId)
		{
			FastPaymentResult result;
			var attemptsCount = 0;

			do
			{
				if(attemptsCount > 0)
				{
					await Task.Delay(60000);
				}

				result = await _fastPaymentSender.SendFastPaymentUrlAsync(orderId, phone, true);

				if(result.Status == ResultStatus.Error && result.OrderAlreadyPaied)
				{
					return true;
				}

				attemptsCount++;

			} while(result.Status == ResultStatus.Error && attemptsCount < 3);

			return result.Status == ResultStatus.Ok;
		}
	}
}
