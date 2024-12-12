using Vodovoz.Domain.Orders;

namespace Vodovoz.Tools.Orders
{
	public class OnlineOrderStateKey : ComparerDeliveryPrice
	{
		private OnlineOrder OnlineOrder { get; set; }

		public override void InitializeFields(OnlineOrder onlineOrder)
		{
			OnlineOrder = onlineOrder;
			DeliveryDate = onlineOrder.DeliveryDate;
			CalculateAllWaterCount(OnlineOrder.OnlineOrderItems);
		}
	}
}
