using System;
using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Factories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Application.Orders.Services;

namespace CustomerOrdersApi.Library.Services
{
	public class CustomerOrdersService : ICustomerOrdersService
	{
		private readonly ILogger<CustomerOrdersService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOnlineOrderFactory _onlineOrderFactory;
		private readonly IOrderService _orderService;

		public CustomerOrdersService(
			ILogger<CustomerOrdersService> logger,
			IUnitOfWork unitOfWork,
			IOnlineOrderFactory onlineOrderFactory,
			IOrderService orderService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_onlineOrderFactory = onlineOrderFactory ?? throw new ArgumentNullException(nameof(onlineOrderFactory));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
		}

		public int CreateOrderFromOnlineOrder(OnlineOrderInfoDto onlineOrderInfoDto)
		{
			var onlineOrder = _onlineOrderFactory.CreateOnlineOrder(_unitOfWork, onlineOrderInfoDto);
			_unitOfWork.Save(onlineOrder);
			_unitOfWork.Commit();
			
			_logger.LogInformation("Создаем заказ из онлайн заказа {OnlineOrderId} и пытаемся его подтвердить", onlineOrder.Id);
			
			return _orderService.TryCreateOrderFromOnlineOrderAndAccept(_unitOfWork, onlineOrder);
		}
	}
}
