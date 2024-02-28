using FastPaymentsApi.Contracts;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace FastPaymentsAPI.Library.Converters
{
	public interface IRequestFromConverter
	{
		PaymentFrom ConvertRequestFromTypeToPaymentFrom(IUnitOfWork uow, RequestFromType requestFromType);
	}
}
