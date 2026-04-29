namespace CustomerOrdersApi.Library.V5.Dto.Orders.Discounts
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
	}
}
