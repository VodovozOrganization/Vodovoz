namespace Vodovoz.Core.Data.Orders.V5
{
	public class DiscountData
	{
		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; set; }
		
		/// <summary>
		/// Скидка в деньгах?
		/// </summary>
		public bool IsDiscountInMoney { get; set; }
		
		/// <summary>
		/// Id скидки/промокода
		/// </summary>
		public int? DiscountReasonId { get; set; }
		
		public static DiscountData Create(
			decimal discount,
			bool isDiscountInMoney,
			int discountReasonId) => new DiscountData
		{
			Discount = discount,
			IsDiscountInMoney = isDiscountInMoney,
			DiscountReasonId = discountReasonId
		};
		
		public static DiscountData Create(
			bool isDiscountInMoney,
			int discountReasonId) => new DiscountData
		{
			IsDiscountInMoney = isDiscountInMoney,
			DiscountReasonId = discountReasonId
		};
		
		public static DiscountData Create(
			decimal discount,
			bool isDiscountInMoney) => new DiscountData
		{
			Discount = discount,
			IsDiscountInMoney = isDiscountInMoney
		};
	}
}
