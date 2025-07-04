using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Requests;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Orders;

namespace FastPaymentsAPI.Library.Validators
{
	public interface IFastPaymentValidator
	{
		string Validate(int orderId, FastPaymentRequestFromType? requestFromType = null);
		string Validate(RequestRegisterOnlineOrderDTO registerOnlineOrderDto, FastPaymentRequestFromType fastPaymentRequestFromType);
		string ValidateOnlineOrder(decimal onlineOrderSum);
		string Validate(int orderId, ref string phoneNumber);
		string Validate(Order order, int orderId);
	}
}
