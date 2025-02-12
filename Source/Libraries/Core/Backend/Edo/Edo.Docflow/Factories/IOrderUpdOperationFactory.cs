using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Edo.Docflow.Factories
{
	public interface IOrderUpdOperationFactory
	{
		OrderUpdOperation CreateOrUpdateOrderUpdOperation(OrderEntity order);
	}
}
