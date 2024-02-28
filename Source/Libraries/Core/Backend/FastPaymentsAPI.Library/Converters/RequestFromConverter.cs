using FastPaymentsApi.Contracts;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace FastPaymentsAPI.Library.Converters
{
	public class RequestFromConverter : IRequestFromConverter
	{
		public PaymentFrom ConvertRequestFromTypeToPaymentFrom(IUnitOfWork uow, RequestFromType requestFromType)
		{
			return uow.GetById<PaymentFrom>((int)requestFromType);
		}
	}
}
