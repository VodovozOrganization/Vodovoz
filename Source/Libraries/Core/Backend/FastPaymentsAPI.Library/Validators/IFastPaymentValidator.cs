using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Requests;
using Vodovoz.Domain.Orders;

namespace FastPaymentsAPI.Library.Validators
{
	public interface IFastPaymentValidator
	{
		string Validate(int orderId, RequestFromType? requestFromType = null);
		string Validate(RequestRegisterOnlineOrderDTO registerOnlineOrderDto, RequestFromType requestFromType);
		string ValidateOnlineOrder(decimal onlineOrderSum);
		string Validate(int orderId, ref string phoneNumber);
		string Validate(Order order, int orderId);
	}
}
