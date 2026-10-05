namespace CustomerOrdersApi.Library.V8.Dto.Orders.Promotions.Discounts
{
	public class DiscountDto
	{
		protected DiscountDto(bool isDiscountInMoney, decimal discount)
		{
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

		public static DiscountDto Create(bool isDiscountInMoney, decimal discount) =>
			new DiscountDto(isDiscountInMoney, discount);
	}
}
