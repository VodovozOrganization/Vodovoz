using Vodovoz.Domain.Orders;

namespace Vodovoz.Tools.Orders
{
	public class OnlineOrderStateKey : DeliveryDateComparerDeliveryPrice
	{
		private OnlineOrder OnlineOrder { get; set; }

		public virtual void InitializeFields(OnlineOrder onlineOrder)
		{
			OnlineOrder = onlineOrder;
			Initialize(OnlineOrder.OnlineOrderItems, onlineOrder.DeliveryDate);
		}
	}
}
