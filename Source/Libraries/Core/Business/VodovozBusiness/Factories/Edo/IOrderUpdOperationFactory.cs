using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Factories.Edo
{
	public interface IOrderUpdOperationFactory
	{
		OrderUpdOperation CreateOrUpdateOrderUpdOperation(Order order, OrderUpdOperation orderUpdOperation = null);
	}
}
