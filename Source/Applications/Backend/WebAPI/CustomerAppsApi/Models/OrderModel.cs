using System;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Orders;

namespace CustomerAppsApi.Models
{
	public class OrderModel : IOrderModel
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOrderRepository _orderRepository;

		public OrderModel(
			IUnitOfWork unitOfWork,
			IOrderRepository orderRepository)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		}

		public bool CanCounterpartyOrderPromoSetForNewClients(int counterpartyId)
		{
			return !_orderRepository.HasCounterpartyFirstRealOrder(_unitOfWork, counterpartyId);
		}
	}
}
