using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IOrderOnlinePaymentAcceptanceHandler
	{
		void AcceptOnlinePayment(
			IUnitOfWork uow,
			IEnumerable<Order> orders,
			int paymentNumber,
			PaymentType paymentType,
			PaymentFrom paymentFrom);
	}
}
