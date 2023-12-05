using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	internal class OrderService
	{
		private readonly ILogger<OrderService> _logger;

		public OrderService(ILogger<OrderService> logger)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
		}

		public void UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order)
		{

		}
	}
}
