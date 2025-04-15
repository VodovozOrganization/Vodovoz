using Vodovoz.Core.Domain.Orders;

namespace Edo.Common
{
	public interface IEdoOrderContactProvider
	{
		string GetContact(OrderEntity order);
	}
}
