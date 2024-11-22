using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
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

namespace RobotMiaApi.Services
{
	/// <summary>
	/// Сервис заказов Api робота Мия
	/// </summary>
	public class OrderService
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
		private readonly IUnitOfWork _unitOfWork;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="nomenclatureSettings"></param>
		/// <param name="roboatsSettings"></param>
		/// <param name="orderRepository"></param>
		/// <param name="unitOfWork"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public OrderService(
			ILogger<OrderService> logger,
			INomenclatureSettings nomenclatureSettings,
			IRoboatsSettings roboatsSettings,
			IGenericRepository<Order> orderRepository,
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
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		/// <summary>
		/// Получение последнего заказа контрагента
		/// </summary>
		/// <param name="counterpartyId"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Получение последнего заказа точки доставки
		/// </summary>
		/// <param name="deliveryPointId"></param>
		/// <returns></returns>
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
	}
}
