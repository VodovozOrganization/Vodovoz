using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;

namespace SmsPaymentService
{
	public interface ISmsPaymentDTOFactory
	{
		SmsPaymentDTO CreateSmsPaymentDTO(IUnitOfWork uow, SmsPayment smsPayment, Order order, PaymentFrom paymentFrom);
	}
}