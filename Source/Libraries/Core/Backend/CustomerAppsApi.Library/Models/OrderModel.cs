using System;
using CustomerAppsApi.Library.Dto.Orders;
using CustomerAppsApi.Library.Factories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Orders;

namespace CustomerAppsApi.Library.Models
{
	public class OrderModel : IOrderModel
	{
		private readonly ILogger<OrderModel> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOrderRepository _orderRepository;
		private readonly IOnlineOrderFactory _onlineOrderFactory;
		private readonly OrderFromOnlineOrderCreator _orderCreator;

		public OrderModel(
			ILogger<OrderModel> logger,
			IUnitOfWork unitOfWork,
			IOrderRepository orderRepository,
			IOnlineOrderFactory onlineOrderFactory,
			OrderFromOnlineOrderCreator orderCreator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_onlineOrderFactory = onlineOrderFactory ?? throw new ArgumentNullException(nameof(onlineOrderFactory));
			_orderCreator = orderCreator ?? throw new ArgumentNullException(nameof(orderCreator));
		}

		public bool CanCounterpartyOrderPromoSetForNewClients(int counterpartyId)
		{
			return !_orderRepository.HasCounterpartyFirstRealOrder(_unitOfWork, counterpartyId);
		}

		public int CreateOrderFromOnlineOrder(OnlineOrderInfoDto onlineOrderInfoDto)
		{
			_logger.LogInformation("Создаем онлайн заказ");
			var onlineOrder = _onlineOrderFactory.CreateOnlineOrder(onlineOrderInfoDto);
			_unitOfWork.Save(onlineOrder);
			_unitOfWork.Commit();
			
			_logger.LogInformation("Создаем заказ из онлайн заказа и пытаемся его подтвердить");
			
			return _orderCreator.CreateOrderFromOnlineOrderAndTryAccept(onlineOrder);
		}
	}
}
