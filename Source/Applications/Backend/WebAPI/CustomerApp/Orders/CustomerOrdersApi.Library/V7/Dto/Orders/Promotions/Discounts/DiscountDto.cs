namespace CustomerOrdersApi.Library.V7.Dto.Orders.Promotions.Discounts
{
	public class DiscountDto
	{
		private DiscountDto(int id, bool isDiscountInMoney, decimal discount)
		{
			DiscountReasonId = id;
			IsDiscountInMoney = isDiscountInMoney;
			Discount = discount;
		}
		
		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		public bool IsDiscountInMoney { get; }
		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; }
		/// <summary>
		/// Id скидки/промокода
		/// </summary>
		public int DiscountReasonId { get; }

		public static DiscountDto Create(int id, bool isDiscountInMoney, decimal discount) =>
			new DiscountDto(id, isDiscountInMoney, discount);
	}
}
