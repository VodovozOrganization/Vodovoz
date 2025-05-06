using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Validation
{
	public interface IFastDeliveryValidator
	{
		Result ValidateOrder(Order order);
	}
}