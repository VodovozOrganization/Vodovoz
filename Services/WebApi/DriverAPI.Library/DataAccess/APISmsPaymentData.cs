using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.Orders;

namespace DriverAPI.Library.DataAccess
{
	public class APISmsPaymentData : IAPISmsPaymentData
	{
		private readonly IOrderRepository orderRepository;
		private readonly IUnitOfWork unitOfWork;

		public APISmsPaymentData(
			IOrderRepository orderRepository,
			IUnitOfWork unitOfWork)
		{
			this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public SmsPaymentStatus? GetOrderPaymentStatus(int orderId)
		{
			return orderRepository.GetOrderPaymentStatus(unitOfWork, orderId);
		}
	}
}
