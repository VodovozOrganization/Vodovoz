using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RobotMiaApi.Contracts.Requests.V1;
using RobotMiaApi.Contracts.Responses.V1;
using RobotMiaApi.Extensions.Mapping;
using RobotMiaApi.Specifications;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain.Orders;
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
			IUnitOfWork unitOfWork)
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
		public int CreateAndAcceptOrder(CreateOrderRequest createOrderRequest)
		{
			return _vodovozOrderService.CreateAndAcceptOrder(createOrderRequest.MapToCreateOrderRequest());
		}

		/// <inheritdoc/>
		public (decimal orderPrice, decimal deliveryPrice, decimal forfeitPrice) GetOrderAndDeliveryPrices(CalculatePriceRequest calculatePriceRequest)
		{
			return _vodovozOrderService.GetOrderAndDeliveryPrices(calculatePriceRequest.MapToCreateOrderRequest());
		}
	}
}
