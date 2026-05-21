namespace CustomerOrders.Contracts.V5.Orders.PromoSets
{
	public class OrderPromoSetDto : PromoSetDto
	{
		public static OrderPromoSetDto Create(
			int promoSetId,
			int count,
			decimal price)
		{
			return new OrderPromoSetDto
			{
				PromoSetId = promoSetId,
				Count = count,
				Price = price
			};
		}
	}
}
