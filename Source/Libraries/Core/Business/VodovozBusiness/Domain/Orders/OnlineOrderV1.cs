using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public class OnlineOrderV1 : OnlineOrder
	{
		/// <inheritdoc/>>
		public override OnlineOrderVersion OrderVersion => OnlineOrderVersion.V1;
	}
}
