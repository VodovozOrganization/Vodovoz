using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.FastPayments;

namespace DriverAPI.Library.Models
{
	public class FastPaymentModel : IFastPaymentModel
	{
		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly IUnitOfWork _uow;

		public FastPaymentModel(
			IFastPaymentRepository fastPaymentRepository,
			IUnitOfWork uow)
		{
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public FastPaymentStatus? GetOrderFastPaymentStatus(int orderId, int? onlineOrder = null)
		{
			onlineOrder ??= _uow.GetById<Order>(orderId).OnlineOrder;

			return _fastPaymentRepository.GetOrderFastPaymentStatus(_uow, orderId, onlineOrder);
		}
	}
}
