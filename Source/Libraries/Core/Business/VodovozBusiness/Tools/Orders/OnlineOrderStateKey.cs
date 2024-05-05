using Vodovoz.Domain.Orders;

namespace Vodovoz.Tools.Orders
{
	public class OnlineOrderStateKey : ComparerDeliveryPrice
	{
		private OnlineOrder OnlineOrder { get; }

		public OnlineOrderStateKey(OnlineOrder onlineOrder) : base(onlineOrder.DeliveryDate)
		{
			OnlineOrder = onlineOrder;
			InitializeFields();
		}

		private void InitializeFields()
		{
			CalculateAllWaterCount(OnlineOrder.OnlineOrderItems);
		}
	}
}
