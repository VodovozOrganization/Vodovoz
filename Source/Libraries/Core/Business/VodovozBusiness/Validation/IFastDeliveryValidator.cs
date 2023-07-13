using Vodovoz.Domain.Orders;
using Vodovoz.Errors;

namespace Vodovoz.Validation
{
	public interface IFastDeliveryValidator
	{
		Result ValidateOrder(Order order);
	}
}