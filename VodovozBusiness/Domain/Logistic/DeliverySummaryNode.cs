using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Logistic
{
	public class DeliverySummaryNode: PropertyChangedBase
	{
		private OrderStatus orderStatus;

		public OrderStatus OrderStatus
		{
			get => orderStatus;
			set => SetField(ref orderStatus, value);
		}

		private TareVolume tareVolume;
		public TareVolume TareVolume 
		{
			get => tareVolume;
			set => SetField(ref tareVolume, value);
		}

		private decimal bottles;

		public decimal Bottles
		{
			get => bottles;
			set => SetField(ref bottles, value);
		}
	}
}