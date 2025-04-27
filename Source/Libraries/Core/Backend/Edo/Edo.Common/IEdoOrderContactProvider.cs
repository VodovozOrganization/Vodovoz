using Vodovoz.Core.Domain.Orders;

namespace Edo.Common
{
	public interface IEdoOrderContactProvider
	{
		EdoOrderAnyContact GetContact(OrderEntity order);
	}
}
