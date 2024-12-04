using System;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Orders;

namespace CustomerAppsApi.Library.Models
{
	public class OrderModel : IOrderModel
	{
		private readonly ILogger<OrderModel> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOrderRepository _orderRepository;

		public OrderModel(
			ILogger<OrderModel> logger,
			IUnitOfWork unitOfWork,
			IOrderRepository orderRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		}

		public bool CanCounterpartyOrderPromoSetForNewClients(int counterpartyId)
		{
			var counterparty = _unitOfWork.GetById<Counterparty>(counterpartyId);
			
			return !_orderRepository.HasCounterpartyFirstRealOrder(_unitOfWork, counterparty);
		}
	}
}
