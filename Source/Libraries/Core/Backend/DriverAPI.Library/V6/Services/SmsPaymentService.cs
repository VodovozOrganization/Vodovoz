using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.Orders;

namespace DriverAPI.Library.V6.Services
{
	internal class SmsPaymentService : ISmsPaymentService
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IUnitOfWork _unitOfWork;

		public SmsPaymentService(
			IOrderRepository orderRepository,
			IUnitOfWork unitOfWork)
		{
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public SmsPaymentStatus? GetOrderSmsPaymentStatus(int orderId)
		{
			return _orderRepository.GetOrderSmsPaymentStatus(_unitOfWork, orderId);
		}
	}
}
