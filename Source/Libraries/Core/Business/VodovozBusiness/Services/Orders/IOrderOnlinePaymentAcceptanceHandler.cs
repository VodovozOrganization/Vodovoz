using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IOrderOnlinePaymentAcceptanceHandler
	{
		void AcceptOnlinePayment(
			IUnitOfWork uow,
			Order order,
			int paymentNumber,
			PaymentType paymentType,
			PaymentFrom paymentFrom);
	}
}
