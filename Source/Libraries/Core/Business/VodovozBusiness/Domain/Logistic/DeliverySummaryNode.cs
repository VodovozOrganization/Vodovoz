using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Logistic
{
	public class DeliverySummaryNode
	{
		private OrderStatus orderStatus;

		public OrderStatus OrderStatus
		{
			get => orderStatus;
			set => orderStatus = value;
		}

		private TareVolume tareVolume;
		public TareVolume TareVolume 
		{
			get => tareVolume;
			set => tareVolume = value;
		}

		private decimal bottles;

		public decimal Bottles
		{
			get => bottles;
			set => bottles = value;
		}
	}
}