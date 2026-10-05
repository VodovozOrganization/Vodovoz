namespace CustomerOrdersApi.Library.V8.Dto.Orders.Promotions.Discounts
{
	public class DiscountWithIdDto : DiscountDto
	{
		protected DiscountWithIdDto(int id, bool isDiscountInMoney, decimal discount) : base(isDiscountInMoney, discount)
		{
			DiscountReasonId = id;
		}
		
		/// <summary>
		/// Id скидки/промокода
		/// </summary>
		public int DiscountReasonId { get; }
		
		public static DiscountWithIdDto Create(int id, bool isDiscountInMoney, decimal discount) =>
			new DiscountWithIdDto(id, isDiscountInMoney, discount);
	}
}
