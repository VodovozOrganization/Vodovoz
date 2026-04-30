namespace CustomerOrders.Contracts.V5.Orders.Discounts
{
	public class DiscountDto
	{
		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		public bool IsDiscountInMoney { get; set; }
		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; set; }
		/// <summary>
		/// Id скидки/промокода
		/// </summary>
		public int? DiscountReasonId { get; set; }
		
		public static DiscountDto Create(bool isDiscountInMoney, decimal discount, int? discountReasonId)
		{
			return new DiscountDto
			{
				IsDiscountInMoney = isDiscountInMoney,
				Discount = discount,
				DiscountReasonId = discountReasonId
			};
		}
	}
}
