using Vodovoz.Core.Domain.Orders;

namespace Receipt.Dispatcher.Tests
{
	public class OrderItemEntityFixture : OrderItemEntity
	{
		public OrderItemEntityFixture()
		{
			
		}

		public void SetCount(decimal count) => Count = count;
		public void SetPrice(decimal price) => Price = price;
		public void SetMoneyDiscount(decimal discountMoney) => DiscountMoney = discountMoney;
	}
}
