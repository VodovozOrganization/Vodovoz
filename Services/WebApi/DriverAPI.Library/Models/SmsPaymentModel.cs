using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.Orders;

namespace DriverAPI.Library.Models
{
	public class SmsPaymentModel : ISmsPaymentModel
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IUnitOfWork _unitOfWork;

		public SmsPaymentModel(
			IOrderRepository orderRepository,
			IUnitOfWork unitOfWork)
		{
			this._orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			this._unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public SmsPaymentStatus? GetOrderPaymentStatus(int orderId)
		{
			return _orderRepository.GetOrderPaymentStatus(_unitOfWork, orderId);
		}
	}
}
